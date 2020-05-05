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
        public abstract string Name { get; set; }
        public abstract int Id { get; protected set; }
        public abstract string Ip { get; protected set; }
        public abstract int Port { get; protected set; }
        public abstract void Dispose();
    }
}
