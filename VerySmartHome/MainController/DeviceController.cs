using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    abstract class DeviceController
    {
        public virtual string DeviceType { get; } = "Device";
        public abstract string DeviceMSearchMessage { get; }
        protected abstract LinkedList<Device> Devices { get; set; }
    }
}
