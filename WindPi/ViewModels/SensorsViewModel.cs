using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using WindPi.Helpers;
using WindPi.Models;

namespace WindPi.ViewModels
{
    public class SensorsViewModel
    {
        private const string IotHubUri = "WindHub.azure-devices.net";

        private const string DeviceId = "RPi-Wind-1";

        private const string DeviceKey = "aaRE0MhoR5XQBZTecv3VO5mfPymgOJtmbYn4ZusGlzU=";

        private readonly DeviceClient _deviceClient;

        public SensorsData Sensors { get; }
        public WindData Wind { get; }

        public string Version { get; }       

        public SensorsViewModel()
        {
            Version = Debugging.GetAppVersion();

            Sensors = new SensorsData();
            Wind = new WindData {Running = true};

            _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Http1);
                        
        }

        public async void SendDeviceToCloudMessagesAsync()
        {            
            var rand = new Random();

            Wind.CurrentWindSpeed = Wind.CurrentWindSpeed + rand.NextDouble() * 4 - 2;

            if (Wind.CurrentWindSpeed < 5) Wind.CurrentWindSpeed = 5;
            if (Wind.CurrentWindSpeed > 30) Wind.CurrentWindSpeed = 30;

            //// Samo za TEST ugašanja!!!!
            //if (Wind.CurrentWindSpeed > 20) Wind.Running = false;

            if (Wind.Running)
            {            
                Wind.EffectiveWindSpeed = Wind.CurrentWindSpeed;
            }
            else
            {
                if (Wind.EffectiveWindSpeed > 0) Wind.EffectiveWindSpeed -= 0.6;
                if (Wind.EffectiveWindSpeed < 0) Wind.EffectiveWindSpeed = 0;
            }

            var telemetryDataPoint = new
            {
                deviceId = DeviceId,
                windSpeed = Wind.CurrentWindSpeed,
                powerOutput = Wind.PowerOutput,
                temperatue = Sensors.Temperature,
                light = Sensors.Lightness
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);            
        }

        public async void ReceiveCloudToDeviceAsync()
        {
            var receivedMessage = await _deviceClient.ReceiveAsync();
            if (receivedMessage == null) return;

            var message = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            
            await _deviceClient.CompleteAsync(receivedMessage);

            ProcessMessage(message);
        }

        private void ProcessMessage(string message)
        {
            var command = (CloudCommand)JsonConvert.DeserializeObject(message);
            switch (command.Command)
            {
                case "STOP":
                    Wind.Running = false;
                    break;
                case "START":
                    Wind.Running = true;
                    break;
                
            }
        }
    }
}