using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GHIElectronics.UWP.Shields;
using WindPi.Helpers;
using WindPi.ViewModels;

namespace WindPi.Views
{
    public sealed partial class MainPage : Page
    {
        internal readonly SensorsViewModel ViewModel = new SensorsViewModel();

        private FEZHAT _hat;
        private DispatcherTimer _timer;
        private bool _tick;

        private readonly FEZHAT.Color _colorA = FEZHAT.Color.Blue;
        private readonly FEZHAT.Color _colorB = FEZHAT.Color.Green;

        public MainPage()
        {
            SetupHat();

            InitializeComponent();

            Debug.WriteLine(Debugging.GetDisplayInfo());
        }
        
        private async void SetupHat()
        {
            _hat = await FEZHAT.CreateAsync();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(3000)};
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _tick = !_tick;

            // blink, blink
            if (ViewModel.Wind.Running)
            {
                _hat.D2.Color = _tick ? _colorA : _colorB;
                _hat.D3.Color = _tick ? _colorB : _colorA;
            }
            else if (ViewModel.Wind.PowerOutput > 0)
            {
                _hat.D2.Color = _tick ? FEZHAT.Color.Red : FEZHAT.Color.Black;
                _hat.D3.Color = _tick ? FEZHAT.Color.Black : FEZHAT.Color.Red;
            }
            else
            {
                _hat.D2.Color = FEZHAT.Color.Red;
                _hat.D3.Color = FEZHAT.Color.Red;
            }

            // Light & Temp sensor
            ViewModel.Sensors.Lightness = _hat.GetLightLevel();                        
            ViewModel.Sensors.Temperature = _hat.GetTemperature();
            
            Debug.WriteLine($"Temperature: {ViewModel.Sensors.Temperature:N2} °C, Light {ViewModel.Sensors.Lightness:P2}");

            // Send Data to Cloud
            ViewModel.SendDeviceToCloudMessagesAsync();

            // Receive Messages from Cloud
            ViewModel.ReceiveCloudToDeviceAsync();

            // Turn on/off from buttons on Hat
            if (_hat.IsDIO18Pressed() || _hat.IsDIO22Pressed())
            {
                ViewModel.Wind.Running = !ViewModel.Wind.Running;
            }     
        }

        
    }
}
