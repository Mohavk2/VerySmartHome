using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.Interfaces;

namespace VerySmartHome.MainController
{
    public abstract class Device : INotifyPropertyChanged, IHasID, IDisposable
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
