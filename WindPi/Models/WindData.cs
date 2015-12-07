using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindPi.Models
{
    public class WindData : INotifyPropertyChanged
    {
        private double _currentWindSpeed;
        private double _effectiveWindSpeed;
        private bool _running;

        public double CurrentWindSpeed
        {
            get { return _currentWindSpeed; }
            set {
                _currentWindSpeed = value;
                OnPropertyChanged();                
            }
        }

        public double EffectiveWindSpeed
        {
            get { return _effectiveWindSpeed; }
            set
            {
                _effectiveWindSpeed = value;
                OnPropertyChanged();
                OnPropertyChanged("PowerOutput");
            }
        }

        public double PowerOutput
        {
            get
            {
                if (EffectiveWindSpeed <= 0.0) return 0;
                return EffectiveWindSpeed / 3.5 + 1.0;
            }
        }
        public bool Running {
            get { return _running; }
            set
            {
                if (_running == value) return;
                _running = value;
                OnPropertyChanged();
            }
        }    

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}