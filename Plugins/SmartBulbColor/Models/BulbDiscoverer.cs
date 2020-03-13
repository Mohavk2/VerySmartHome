using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    sealed class BulbDiscoverer : DeviceDiscoverer
    {
        public BulbDiscoverer(string message) : base(message) { }
        public BulbDiscoverer(string message, string ip, int port) : base(message, ip, port) { }
        protected override IDevice CreateDevice(string response)
        {
            return new BulbColor(response);
        }
    }
}
