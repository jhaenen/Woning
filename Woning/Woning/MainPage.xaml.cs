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
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public MainPage() {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            GetLamps();
            foreach (Lamp lamp in LampCollection) {
                Debug.WriteLine(lamp.Name);
            }
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
                            LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString()));
                        else if(lampData.SwitchType == "Dimmer") {
                            if (lampData.Type == "Light/Switch") LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), true));
                            if (lampData.Type == "Color Switch") LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), true, true));
                        }
                    }
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
