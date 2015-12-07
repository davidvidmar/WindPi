using System.ComponentModel;
using System.Runtime.CompilerServices;
using WindPi.Helpers;

namespace WindPi.Models
{
    public class DebugData : INotifyPropertyChanged
    {
        public string Version { get; }
        public string LocalIP { get; }

        private string _lastMessageSent;
        public string LastMessageSent
        {
            get { return _lastMessageSent; }
            set
            {
                _lastMessageSent = value;
                OnPropertyChanged();
            }
        }

        public DebugData()
        {
            Version = "Version: " + Debugging.GetAppVersion();
            LocalIP = "Local IP: " + Debugging.GetLocalIp();
            LastMessageSent = "-";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
