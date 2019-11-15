using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    abstract class DeviceController
    {
        protected abstract IEnumerable<Device> Devices { get; set; }
    }
}
