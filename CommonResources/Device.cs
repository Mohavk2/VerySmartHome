using System;
using System.ComponentModel;

namespace CommonLibrary
{
    public abstract class Device : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public abstract string GetName();
        public abstract int GetId();
        public abstract string GetIP();
        public abstract int GetPort();
        public abstract void DisconnectMusicMode();
        public abstract void Dispose();
    }
}
