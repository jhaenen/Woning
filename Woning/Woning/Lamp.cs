using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Woning {
    class Lamp : INotifyPropertyChanged {
        public uint IDX { get; internal set; }
        public string Name { get; internal set; }

        private string imguri { get; set; }
        public string ImageUri {
            get {
                return imguri;
            }
            set {
                imguri = value;
                NotifyPropertyChanged();
            }
        }

        public bool Dimmable { get; internal set; }
        public bool ColorLamp { get; internal set; }
        private bool status { get; set; }

        public bool Status {
            get {
                return status;
            }
            set {
                status = value;
                if(status) ImageUri = "Images/lamp-on.svg";
                else ImageUri = "Images/lamp-off.svg";
                NotifyPropertyChanged();
                NotifyPropertyChanged("ImageUri");
            }
        }
        private uint brightness { get; set; }
        public uint Brightness {
            get {
                return brightness;
            }
            set {
                brightness = value;
                NotifyPropertyChanged();
            }
        }
        public Brush ColorBrush { get; set; }
        private Color lampcolor { get; set; }
        public Color LampColor { 
            get {
                return lampcolor;
            }
            set {
                lampcolor = value;
                ColorBrush = new SolidColorBrush(lampcolor);
                NotifyPropertyChanged();
                NotifyPropertyChanged("ColorBrush");
            }
        }

        enum ChangeMode { none, colorMode, brighnessMode, switchMode }
        ChangeMode changeMode = ChangeMode.none;
        DispatcherTimer changeTimer = new DispatcherTimer();

        private async void changeTimer_Tick(object sender, object e) {
            Debug.WriteLine("Tick");
            if(changeMode == ChangeMode.colorMode) {
                string hex = LampColor.ToString().Substring(3, 6);
                if (Brightness == 0) {
                    if(!Status) Status = true;
                    Brightness = 100;
                }
                await GetAsync($"http://192.168.2.210/json.htm?type=command&param=setcolbrightnessvalue&idx={IDX}&hex={hex}&brightness={Brightness}&iswhite=false");
            } else if(changeMode == ChangeMode.brighnessMode) {
                if (Brightness == 0) {
                    if (Status) Status = false;
                } else {
                    if (!Status) Status = true;
                }
                if (!ColorLamp) await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Set%20Level&level={Brightness}");
                else {
                    string hex = LampColor.ToString().Substring(3, 6);
                    await GetAsync($"http://192.168.2.210/json.htm?type=command&param=setcolbrightnessvalue&idx={IDX}&hex={hex}&brightness={Brightness}&iswhite=false");
                }
            }
            changeMode = ChangeMode.none;
            changeTimer.Stop();
        }

        public Lamp(uint idx, string name, string status, bool dimmable) {
            IDX = idx;
            Name = name;
            Dimmable = dimmable;
            if (status == "Off") {
                Status = false;
            } else {
                Status = true;

                if(dimmable) {
                    Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                }
            }

            changeTimer.Interval = TimeSpan.FromMilliseconds(200);
            changeTimer.Tick += changeTimer_Tick;
        }

        public Lamp(uint idx, string name, string status, string colorStr) {
            IDX = idx;
            Name = name;
            Dimmable = true;
            dynamic json = JsonConvert.DeserializeObject(colorStr);
            if (json.r == 0 && json.g == 0 && json.b == 0) {
                ColorLamp = false;
            } else {
                ColorLamp = true;
                LampColor = Color.FromArgb(255, (byte)json.r, (byte)json.g, (byte)json.b);
            }
            if (status == "Off") {
                Status = false;
            } else {
                Status = true;
                Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
            }

            changeTimer.Interval = TimeSpan.FromMilliseconds(200);
            changeTimer.Tick += changeTimer_Tick;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static async Task<string> GetAsync(string uri) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }

        public async void Switch() {
            if (Status) {
                Status = false;
                if (Dimmable) Brightness = 0;
                await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Off");
            } else {
                if (Dimmable) {
                    Brightness = 100;
                    Status = true;
                    await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Set%20Level&level=100");
                } else {
                    Status = true;
                    await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=On");
                }
            }
            changeMode = ChangeMode.switchMode;
        }

        public void SetStatus(uint status) {
            if (status == 0) {
                if (Status) Status = false;
            } else {
                if (!Status) Status = true;
            }
        }

        public void SetStatus(uint status, uint brightness) {
            if(status == 0) {
                if (Status) {
                    Brightness = 0;
                    Status = false;
                }
            } else {
                if (!Status || Brightness != brightness) {
                    Brightness = brightness;
                    Status = true;
                }
            }
        }

        public void SetStatus(uint status, uint brightness, byte r, byte g, byte b) {
            Color newColor = Color.FromArgb(255, r, g, b);
            if (status == 0) {
                if (Status) {
                    Brightness = 0;
                    Status = false;
                }
            } else {
                if (!Status || Brightness != brightness || LampColor != newColor) {
                    Brightness = brightness;
                    Status = true;
                    LampColor = newColor;
                }
            }
        }

        public void SetColor(ColorPicker sender, ColorChangedEventArgs args) {
            LampColor = args.NewColor;
            changeMode = ChangeMode.colorMode;
            changeTimer.Start();
        }

        public void SetBrightnessSlide(object sender, RangeBaseValueChangedEventArgs e) {
            Slider slider = sender as Slider;
            Brightness = (uint)slider.Value;
            if (changeMode != ChangeMode.switchMode) {
                changeMode = ChangeMode.brighnessMode;
                changeTimer.Start();
            } else {
                changeMode = ChangeMode.none;
            }
        }
    }
}
