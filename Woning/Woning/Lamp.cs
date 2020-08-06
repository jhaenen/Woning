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

        public Lamp(uint idx, string name, string status, bool dimmable, bool colorLamp) {
            IDX = idx;
            Name = name;
            //Color = new float[3];
            Dimmable = dimmable;
            ColorLamp = colorLamp;
            if (status == "Off") {
                Status = false;
            } else {
                Status = true;

                if(dimmable) {
                    Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                }
            }
        }

        public Lamp(uint idx, string name, string status, string colorStr) {
            IDX = idx;
            Name = name;
            Dimmable = true;
            ColorLamp = true;
            dynamic json = JsonConvert.DeserializeObject(colorStr);
            LampColor = Color.FromArgb(255, (byte)json.r, (byte)json.g, (byte)json.b);
            if (status == "Off") {
                Status = false;
            } else {
                Status = true;
                Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
            }
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
        }
        //public void SetColor(float r, float g, float b) { if (ColorLamp) { Color[0] = r; Color[1] = g; Color[2] = b; } }

        public void SetStatus(uint status, uint brightness) {
            if(status == 0) {
                if (Status) {
                    if (Dimmable) Brightness = 0;
                    Status = false;
                }
            } else {
                if (!Status || Brightness != brightness) {
                    if (Dimmable) Brightness = brightness;
                    Status = true;
                }
            }
        }

        public void SetColor(ColorPicker sender, ColorChangedEventArgs args) {
            LampColor = args.NewColor;
        }

        public async void UpdateColor(object sender, ManipulationCompletedRoutedEventArgs e) {
            string hex = LampColor.ToString().Substring(3, 6);
            Debug.WriteLine(hex);
            if(Brightness == 0) {
                Status = true;
                Brightness = 100;
            }
            await GetAsync($"http://192.168.2.210/json.htm?type=command&param=setcolbrightnessvalue&idx={IDX}&hex={hex}&brightness={Brightness}&iswhite=false");
        }

        public async void SetBrightnessSlide(object sender, ManipulationCompletedRoutedEventArgs e) {
            Slider slider = sender as Slider;
            if (slider.Value == 0) {
                if (Status) {
                    Status = false;
                }
            } else {
                if (!Status) {
                    Status = true;
                }
            }
            Brightness = (uint)slider.Value;
            await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Set%20Level&level={(uint)slider.Value}");
        }

        public async void SetBrightnessTapped(object sender, RangeBaseValueChangedEventArgs e) {
            if(Math.Abs(e.NewValue - e.OldValue) > 10) {
                if (e.NewValue == 0) {
                    if (Status) {
                        Status = false;
                    }
                } else {
                    if (!Status) {
                        Status = true;
                    }
                }
                Brightness = (uint)e.NewValue;
                await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Set%20Level&level={(uint)e.NewValue}");
            }
        }
    }
}
