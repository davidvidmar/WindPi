using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindyPi.Models
{
    public class WindData : INotifyPropertyChanged
    {
        private double _currentWindSpeed;

        public double CurrentWindSpeed
        {
            get { return _currentWindSpeed; }
            set { _currentWindSpeed = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}