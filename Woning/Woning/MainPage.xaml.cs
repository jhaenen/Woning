﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Woning {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e) {
            string response = await GetAsync(@"http://192.168.2.210/json.htm?type=command&param=getlightswitches");
            dynamic lamps = JsonConvert.DeserializeObject(response);
            if (lamps.status != "OK") {
                test.Text = "Error";
                return;
            }
            foreach(dynamic entry in lamps.result) {
                if (entry.Type == "Light/Switch" || entry.Type == "Color Switch") {
                    TextBlock panel = await createEntry((uint)entry.idx);
                    if(panel != null) stack.Children.Add(panel);
                }
            }
        }

        private async Task<TextBlock> createEntry(uint idx) {
            string response = await GetAsync(@"http://192.168.2.210/json.htm?type=devices&rid=" + idx.ToString());
            dynamic json = JsonConvert.DeserializeObject(response);
            dynamic lampData = json.result[0];

            if (lampData.SwitchType != "On/Off" && lampData.SwitchType != "Dimmer") return null;

            DimmableLamp lamp = new DimmableLamp(idx, lampData.Name.ToString());
            lamp.Switch(false);

            TextBlock txt = new TextBlock();
            txt.HorizontalAlignment = HorizontalAlignment.Center;

            
            txt.Text = idx + ": " + lampData.Name + "(" + lampData.Status + ")";

            return txt;
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
