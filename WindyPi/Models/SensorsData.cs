using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindyPi.Models
{
    public class SensorsData : INotifyPropertyChanged
    {
        private double _lightness;
        private double _temperature;

        public double Lightness
        {
            get { return _lightness; }
            set { _lightness = value; OnPropertyChanged(); }
        }

        public double Temperature
        {
            get { return _temperature; }
            set { _temperature = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
