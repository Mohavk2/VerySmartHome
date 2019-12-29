using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public abstract class Device
    {
        public abstract int ID { get; set; }
        public abstract string Name { get; set; }
        public abstract int IP { get; set; }
        public abstract int Port { get; set; }
        public abstract bool IsConnected { get; set; }
        public abstract bool IsPowered { get; set; }
    }
}
