using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
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
using System.Text;
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

        IMqttClient mqttClient;

        private const string DomoticzUrl = "192.168.2.210";

        public MainPage() {
            this.InitializeComponent();

            GetLamps();

            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(DomoticzUrl, 1883)
                .Build();

            System.Threading.CancellationToken cancellationToken;
            mqttClient.ConnectAsync(options, cancellationToken);

            mqttClient.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### CONNECTED WITH SERVER ###");

                MqttTopicFilter topicFilter = new MqttTopicFilter();
                topicFilter.Topic = "domoticz/out";

                await mqttClient.SubscribeAsync(topicFilter);

                Debug.WriteLine("### SUBSCRIBED ###");
            });

            mqttClient.UseApplicationMessageReceivedHandler(async e => {
                dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                Lamp lamp = LampCollection.FirstOrDefault(lc => lc.IDX == (uint)json.idx);
                if (lamp != null) {
                    Debug.WriteLine($"{lamp.Name} has updated! NValue: {json.nvalue}, SValue: {json.svalue1}");
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => {
                        lamp.SetStatus((uint)json.nvalue, (uint)json.svalue1);
                    });
                }
            });
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
                            if (lampData.Type == "Color Switch") LampCollection.Add(new Lamp((uint)entry.idx, entry.Name.ToString(), lampData.Status.ToString(), lampData.Color.ToString()));
                        }
                    }
                }
            }
        }

        public static async Task<string> GetAsync(string uri) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }


        private void Lamp_Click(object sender, ItemClickEventArgs e) {
            Lamp lamp = e.ClickedItem as Lamp;
            lamp.Switch();
        }
    }

    
}
