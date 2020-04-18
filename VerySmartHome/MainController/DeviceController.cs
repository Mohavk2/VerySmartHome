using System.Collections.Generic;

namespace VerySmartHome.MainController
{
    public abstract class DeviceController
    {
        public abstract int DeviceCount { get;}
        public abstract List<Device> GetDevices();
    }
}
