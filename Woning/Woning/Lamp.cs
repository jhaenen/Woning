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

namespace Woning {
    class Lamp : INotifyPropertyChanged {
        private bool status { get; set; }
        private uint brightness { get; set; }
        private string imguri { get; set; }

        public uint IDX { get; internal set; }
        public string Name { get; internal set; }
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
        public bool Status {
            get {
                return status;
            }
            set {
                status = value;
                NotifyPropertyChanged();
            }
        }
        public uint Brightness {
            get {
                return brightness;
            }
            set {
                brightness = value;
                NotifyPropertyChanged();
            }
        }
        public float[] Color { get; set; }

        public Lamp(uint idx, string name, string status, bool dimmable, bool colorLamp) {
            IDX = idx;
            Name = name;
            Color = new float[3];
            Dimmable = dimmable;
            ColorLamp = colorLamp;
            if (status == "Off") {
                ImageUri = "Images/lamp-off.svg";
                Status = false;
            } else {
                ImageUri = "Images/lamp-on.svg";
                Status = true;

                if(dimmable) {
                    Brightness = uint.Parse(Regex.Match(status, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                }
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
                ImageUri = "Images/lamp-off.svg";
                Status = false;
                if (Dimmable) {
                    Brightness = 0;
                }
                await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Off");
            } else {
                ImageUri = "Images/lamp-on.svg";
                Status = true;
                if (Dimmable) {
                    Brightness = 100;
                    await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=Set%20Level&level=100");
                } else {
                    await GetAsync($"http://192.168.2.210/json.htm?type=command&param=switchlight&idx={IDX}&switchcmd=On");
                }
            }
        }
        public void SetColor(float r, float g, float b) { if (ColorLamp) { Color[0] = r; Color[1] = g; Color[2] = b; } }

        public void SetStatus(uint status, uint brightness) {
            if(status == 0) {
                Brightness = 0;
                Status = false;
                ImageUri = "Images/lamp-off.svg";
            } else {
                Brightness = brightness;
                Status = true;
                ImageUri = "Images/lamp-on.svg";
            }
        }
    }
}
