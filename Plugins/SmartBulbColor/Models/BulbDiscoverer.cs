using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    class BulbDiscoverer : SSDPDiscoverer
    {
        BulbDiscoverer(string message) : base(message) { }
        BulbDiscoverer(string message, string ip, int port) : base(message, ip, port) { }

    }
}
