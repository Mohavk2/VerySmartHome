using System.Collections.Generic;

namespace CommonLibrary
{
    public abstract class DeviceController
    {
        public abstract int DeviceCount { get;}
        public abstract List<Device> GetDevices();
    }
}
