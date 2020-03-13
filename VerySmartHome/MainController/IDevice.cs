using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public interface IDevice : INotifyPropertyChanged, IDisposable
    {
        int GetId();
        string GetName();
        string GetIP();
        int GetPort();
    }
}
