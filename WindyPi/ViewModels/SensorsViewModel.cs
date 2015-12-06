using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using WindyPi.Models;

namespace WindyPi.ViewModels
{
    public class SensorsViewModel
    {
        private const string IotHubUri = "WindHub.azure-devices.net";

        private const string DeviceId = "RPi-Wind-1";
        private const string DeviceKey = "aaRE0MhoR5XQBZTecv3VO5mfPymgOJtmbYn4ZusGlzU=";

        private readonly DeviceClient _deviceClient;

        public SensorsData Sensors { get; private set; }
        public WindData Wind { get; private set; }

        public SensorsViewModel()
        {
            Sensors = new SensorsData();
            Wind = new WindData();

            Wind.CurrentWindSpeed = 10;

            _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Http1);
        }

        public async void SendDeviceToCloudMessagesAsync()
        {            
            var rand = new Random();

            Wind.CurrentWindSpeed = Wind.CurrentWindSpeed + rand.NextDouble() * 4 - 2;

            var telemetryDataPoint = new
            {
                deviceId = "myFirstDevice",
                windSpeed = Wind.CurrentWindSpeed
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);            
        }

    }
}