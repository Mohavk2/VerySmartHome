using System;
using System.ComponentModel;


namespace VerySmartHome.Interfaces
{
    public interface IDevice : INotifyPropertyChanged, IDisposable
    {
        int GetId();
        string GetName();
        string GetIP();
        int GetPort();
        void OnNoResponseFromDevice();
    }
}
