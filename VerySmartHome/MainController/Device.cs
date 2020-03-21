using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public abstract class Device : INotifyPropertyChanged, IDisposable
    {
        public delegate void NoResponseHandler(Device device);
        public static event NoResponseHandler NoResponseFromDevice;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnNoResponseFromDevice()
        {
            NoResponseFromDevice?.Invoke(this);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public abstract int GetId();
        public abstract string GetName();
        public abstract string GetIP();
        public abstract int GetPort();
        public abstract void Disconnect();
        public abstract void Dispose();
    }
}
