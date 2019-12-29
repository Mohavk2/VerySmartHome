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
        public override string SSDPMessage { get; } = "M-SEARCH* HTTP/1.1\r\n" +
                                                      "HOST: 239.255.255.250:1982\r\n" +
                                                      "MAN: \"ssdp:discover\"\r\n" +
                                                      "ST: wifi_bulb";
        protected override LinkedList<Device> Devices { get; set; }
        public bool IsThereBulbs { get; set; }

        public void DiscoverForBulbs()
        {
            SSDPDiscoverer discoverer = new SSDPDiscoverer(SSDPMessage);
            Devices = ParseDevices(discoverer.GetDeviceResponses());
        }

        LinkedList<Device> ParseDevices(List<string> responses)
        {
            LinkedList<Device> devices = new LinkedList<Device>();

            return devices;
        }
        /*(
        private string[] SplitResponse(string response)
        {
            if (Answer != null)
            {
                string[] Prop = Answer.Split('\r');
                for (int i = 0; i < Prop.Length; i++)
                {
                    int ind;
                    string subString = ": ";
                    if ((ind = Prop[i].IndexOf(subString)) < 0)
                    {
                        continue;
                    }
                    Prop[i] = Prop[i].Remove(0, ind + subString.Length);
                }
                return Prop;
            }
            else return null;
        }
        */
    }
}
