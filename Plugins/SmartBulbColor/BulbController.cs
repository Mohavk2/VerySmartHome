using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor
{
    sealed class BulbController : DeviceController
    {
        public override string DeviceType { get; } = "MiBulbColor";
        public override string SSDPMessage { get; } = (
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1982\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: wifi_bulb");
        public LinkedList<Bulb> Bulbs { get; private set; } = new LinkedList<Bulb>();
        public override int DeviceCount { get; protected set; }
        public override LinkedList<Device> GetDevices()
        {
            if (Bulbs.Count != 0)
            {
                LinkedList<Device> devices = new LinkedList<Device>(Bulbs);
                return devices;
            }
            else return new LinkedList<Device>();
        }
        public void DiscoverForBulbs()
        {
            SSDPDiscoverer discoverer = new SSDPDiscoverer(SSDPMessage);
            try
            {
                Bulbs = ParseBulbs(discoverer.GetDeviceResponses());
                DeviceCount = Bulbs.Count;
            }
            catch(Exception NoDeviceException)
            {
                throw NoDeviceException = new Exception("No device found!!!");
            }
        }
        public List<String> GetDeviceReports()
        {
            List<String> reports = new List<string>();
            if (Bulbs.Count != 0)
            {
                foreach (Bulb bulb in Bulbs)
                {
                    reports.Add(bulb.GetReport());
                }
                return reports;
            }
            else
            {
                reports.Add("No color bulbs found yet!!!");
                return reports;
            }
        }
        LinkedList<Bulb> ParseBulbs(List<string> responses)
        {
            LinkedList<Bulb> bulbs = new LinkedList<Bulb>();
            for(int i = 0; i < responses.Count; i++)
            {
                Bulb bulb = Bulb.Parse(responses[i]);
                if (bulb.Model == "color")
                {
                    bulbs.AddLast(bulb);
                }
            }
            return bulbs;
        }

    }
}
