using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public abstract class DeviceController
    {
        public abstract int DeviceCount { get;}
        public abstract List<Device> GetDevices();
    }
}
