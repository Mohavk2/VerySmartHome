using VerySmartHome.Interfaces;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    sealed class BulbDiscoverer : DeviceDiscoverer
    {
        public BulbDiscoverer(string message) : base(message) { }
        public BulbDiscoverer(string message, string ip, int port) : base(message, ip, port) { }
        protected override IDevice CreateDevice(string response)
        {
            return new ColorBulb(response);
        }
    }
}
