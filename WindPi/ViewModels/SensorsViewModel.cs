using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using WindPi.Helpers;
using WindPi.Models;

namespace WindPi.ViewModels
{
    /// <summary>
    /// Class SensorsViewModel for working with sensors and Azure IOT hub
    /// </summary>
    public class SensorsViewModel
    {
        /// <summary>
        /// The _device client
        /// </summary>
        private readonly DeviceClient _deviceClient;

        /// <summary>
        /// Gets the sensors.
        /// </summary>
        /// <value>The sensors.</value>
        public SensorsData Sensors { get; } = new SensorsData();

        /// <summary>
        /// Gets the wind.
        /// </summary>
        /// <value>The wind.</value>
        public WindData Wind { get; } = new WindData { Running = true };

        /// <summary>
        /// Gets the debug.
        /// </summary>
        /// <value>The debug.</value>
        public DebugData Debug { get; } = new DebugData();

        /// <summary>
        /// The hat
        /// </summary>
        private FEZHAT hat;
        /// <summary>
        /// The timer
        /// </summary>
        private DispatcherTimer timer;
        /// <summary>
        /// The tick
        /// </summary>
        private bool tick;

        /// <summary>
        /// The color a
        /// </summary>
        private readonly FEZHAT.Color colorA = FEZHAT.Color.Blue;
        /// <summary>
        /// The color b
        /// </summary>
        private readonly FEZHAT.Color colorB = FEZHAT.Color.Green;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorsViewModel" /> class.
        /// </summary>
        public SensorsViewModel()
        {
            _deviceClient = DeviceClient.Create(AppSettings.IotHubUri,
                new DeviceAuthenticationWithRegistrySymmetricKey(AppSettings.DeviceId, AppSettings.DeviceKey),
                TransportType.Http1);
        }

        /// <summary>
        /// Initializes the sensors asynchronous.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        public async Task InitSensorsAsync()
        {
            hat = await FEZHAT.CreateAsync();

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        /// <summary>
        /// Timer_s the tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private async void Timer_Tick(object sender, object e)
        {
            tick = !tick;

            // blink, blink
            if (Wind.Running)
            {
                hat.D2.Color = tick ? colorA : colorB;
                hat.D3.Color = tick ? colorB : colorA;
            }
            else if (Wind.PowerOutput > 0)
            {
                hat.D2.Color = tick ? FEZHAT.Color.Red : FEZHAT.Color.Black;
                hat.D3.Color = tick ? FEZHAT.Color.Black : FEZHAT.Color.Red;
            }
            else
            {
                hat.D2.Color = FEZHAT.Color.Red;
                hat.D3.Color = FEZHAT.Color.Red;
            }

            // Light & Temp sensor
            Sensors.Lightness = hat.GetLightLevel();
            Sensors.Temperature = hat.GetTemperature();

            //Debug.WriteLine($"Temperature: {Sensors.Temperature:N2} °C, Light {Sensors.Lightness:P2}");

            // Send Data to Cloud
            await SendDeviceToCloudMessagesAsync();

            // Receive Messages from Cloud
            await ReceiveCloudToDeviceAsync();

            // Turn on/off from buttons on Hat
            if (hat.IsDIO18Pressed() || hat.IsDIO22Pressed())
            {
                Wind.Running = !Wind.Running;
            }
        }

        /// <summary>
        /// send device to cloud messages as an asynchronous operation.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        public async Task SendDeviceToCloudMessagesAsync()
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
                deviceId = AppSettings.DeviceId,
                windSpeed = Wind.CurrentWindSpeed,
                powerOutput = Wind.PowerOutput,
                temperatue = Sensors.Temperature,
                light = Sensors.Lightness
            };

            try
            {
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await _deviceClient.SendEventAsync(message);

                System.Diagnostics.Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Debug.LastMessageSent = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.LastMessageSent = "Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now} > Error sending message: {ex.Message}");
            }
        }

        /// <summary>
        /// receive cloud to device as an asynchronous operation.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task ReceiveCloudToDeviceAsync()
        {
            Message receivedMessage;
            try
            {
                receivedMessage = await _deviceClient.ReceiveAsync();

                if (receivedMessage == null) return;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now} > Error sending message: {e.Message}");
                return;
            }

            //the message was received, get the data and convert it to bytes
            string message = Encoding.ASCII.GetString(receivedMessage.GetBytes());

            try
            {
                await _deviceClient.CompleteAsync(receivedMessage); //complete the call
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now} > Error sending message: {e.Message}");
            }

            switch (message)
            {
                case "START":
                    Wind.Running = true;
                    break;
                default:
                    Wind.Running = false;
                    break;
            }
        }
    }
}