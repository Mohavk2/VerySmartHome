using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.RemoteBulbAPI
{
    public static class BulbSsdpOptions
    {
        public const string SsdpMessage = ("M-SEARCH * HTTP/1.1\r\n" + "HOST: 239.255.255.250:1982\r\n" + "MAN: \"ssdp:discover\"\r\n" + "ST: wifi_bulb");
        public const string DeviceType = "MiBulbColor";
        public const int MulticastPort = 1982;
    }
}
