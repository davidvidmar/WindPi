using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using WindPi.Models;

namespace WindPi.ViewModels
{
    public class SensorsViewModel
    {
        private const string IotHubUri = "<insert-your-iothub-url>";
        private const string DeviceId = "<insert-your-registered-device-id>";
        private const string DeviceKey = "<insert-your-registered-device-key>";

        private readonly DeviceClient _deviceClient;

        public SensorsData Sensors { get; }
        public WindData Wind { get; }
        public DebugData Debug { get; }

        public SensorsViewModel()
        {
            Sensors = new SensorsData();
            Wind = new WindData {Running = true};
            Debug = new DebugData();

            try
            {
                _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Http1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IoT Hub initialization failed: " + ex.Message);
            }
            
        }

        public async void SendDeviceToCloudMessagesAsync()
        {            
            var rand = new Random();

            Wind.CurrentWindSpeed = Wind.CurrentWindSpeed + rand.NextDouble() * 4 - 2;

            if (Wind.CurrentWindSpeed < 5) Wind.CurrentWindSpeed = 5;
            if (Wind.CurrentWindSpeed > 30) Wind.CurrentWindSpeed = 30;

            if (Wind.Running)
            {            
                Wind.EffectiveWindSpeed = Wind.CurrentWindSpeed;
            }
            else
            {
                if (Wind.EffectiveWindSpeed > 0) Wind.EffectiveWindSpeed -= 0.9;
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

            try
            {
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                if (_deviceClient != null)
                {
                    await _deviceClient.SendEventAsync(message);
                    System.Diagnostics.Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    Debug.LastMessageSent = DateTime.Now.ToString();
                }
                else
                {
                    Debug.LastMessageSent = "IoT Hub is not initialized. Nothing is being sent.";
                    System.Diagnostics.Debug.WriteLine("IoT Hub is not initialized. Nothing is being sent.");
                }
            }
            catch (Exception ex)
            {
                Debug.LastMessageSent = "Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("{0} > Error sending message: {1}", DateTime.Now, ex.Message);
            }
            
        }

        public async void ReceiveCloudToDeviceAsync()
        {
            // bail out if IoT hub is not initialized
            if (_deviceClient == null) return;

            // receive messages
            var receivedMessage = await _deviceClient.ReceiveAsync();
            if (receivedMessage == null) return;

            var message = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            
            await _deviceClient.CompleteAsync(receivedMessage);

            ProcessMessage(message);
        }

        private void ProcessMessage(string message)
        {            
            switch (message)
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