using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Woning {
    public sealed partial class MainPage : Page {

        ObservableCollection<Lamp> LampCollection { get; set; } = new ObservableCollection<Lamp>();
        DispatcherTimer timer = new DispatcherTimer();

        public MainPage() {
            this.InitializeComponent();

            GetLamps();
            
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler<object>(UpdateLamps);
        }

        private async void GetLamps() {
            string response = await GetAsync(@"http://192.168.2.210/json.htm?type=command&param=getlightswitches");
            dynamic lamps = JsonConvert.DeserializeObject(response);
            if (lamps.status == "OK") {
                foreach (dynamic entry in lamps.result) {
                    if (entry.Type == "Light/Switch" || entry.Type == "Color Switch") {
                        response = await GetAsync(@"http://192.168.2.210/json.htm?type=devices&rid=" + entry.idx.ToString());
                        dynamic json = JsonConvert.DeserializeObject(response);
                        dynamic lampData = json.result[0];

                        if (lampData.SwitchType == "On/Off")
                            LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), lampData.Status.ToString(), false, false));
                        else if(lampData.SwitchType == "Dimmer") {
                            if (lampData.Type == "Light/Switch") LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), lampData.Status.ToString(), true, false));
                            if (lampData.Type == "Color Switch") LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), lampData.Status.ToString(), true, true));
                        }
                    }
                }
            }

            timer.Start();
        }

        private async void UpdateLamps(object sender, object e) {
            foreach (Lamp lamp in LampCollection) {
                string response = await GetAsync(@"http://192.168.2.210/json.htm?type=devices&rid=" + lamp.IDX.ToString());
                dynamic json = JsonConvert.DeserializeObject(response);
                if (json.status == "OK") {
                    lamp.SetStatus(json.result[0].Status.ToString());
                }
            }
        }

        private async Task<string> GetAsync(string uri) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }
    }

    
}
