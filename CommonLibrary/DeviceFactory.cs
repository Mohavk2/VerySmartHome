using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary
{
    public abstract class DeviceFactory
    {
        public abstract Device CreateDevice(string response);
    }
}
