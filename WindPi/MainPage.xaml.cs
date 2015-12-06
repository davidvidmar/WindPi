using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using WindPi.Helpers;
using WindPi.ViewModels;

using GHIElectronics.UWP.Shields;

namespace WindPi
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

            Debugging.WriteDisplayInfo();
        }
        
        private async void SetupHat()
        {
            _hat = await FEZHAT.CreateAsync();

            _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _tick = !_tick;

            // blink, blink
            _hat.D2.Color = _tick ? _colorA : _colorB;
            _hat.D3.Color = _tick ? _colorB : _colorA;
            
            // Light & Temp sensor
            ViewModel.Sensors.Lightness = _hat.GetLightLevel();                        
            ViewModel.Sensors.Temperature = _hat.GetTemperature();
            
            Debug.WriteLine($"Temperature: {ViewModel.Sensors.Temperature:N2} °C, Light {ViewModel.Sensors.Lightness:P2}");

            // Wind
            ViewModel.SendDeviceToCloudMessagesAsync();

        }
    }
}
