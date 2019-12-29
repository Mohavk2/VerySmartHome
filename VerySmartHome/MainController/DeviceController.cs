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
        public virtual string DeviceType { get; } = "Device";
        public abstract string SSDPMessage { get; }
        protected abstract LinkedList<Device> Devices { get; set; }
    }
}
