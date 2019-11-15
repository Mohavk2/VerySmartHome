using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    abstract class Device
    {
        protected abstract int ID { get; set; }
        protected abstract int IP { get; set; }
        protected abstract int Port { get; set; }
    }
}
