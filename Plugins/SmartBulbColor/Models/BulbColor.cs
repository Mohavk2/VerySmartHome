using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    internal sealed class BulbColor : Device
    {
        private const string DeviceType = "MiBulbColor";
        private const string SSDPMessage = ("M-SEARCH * HTTP/1.1\r\n"+"HOST: 239.255.255.250:1982\r\n"+"MAN: \"ssdp:discover\"\r\n"+"ST: wifi_bulb");
        private const string LOCATION = "Location: yeelight://";
        private const string ID = "id: ";
        private const string MODEL = "model: ";
        private const string FW_VER = "fw_ver: ";
        private const string POWER = "power: ";
        private const string BRIGHT = "bright: ";
        private const string COLOR_MODE = "color_mode: ";
        private const string CT = "ct: ";
        private const string RGB = "rgb: ";
        private const string HUE = "hue: ";
        private const string SAT = "sat: ";
        private const string NAME = "name: ";

        public static LinkedList<BulbColor> DiscoverBulbs()
        {
            return ParseBulbs(Discoverer.GetDeviceResponses());
        }

        private static SSDPDiscoverer Discoverer = new SSDPDiscoverer(SSDPMessage);
        private static LinkedList<BulbColor> ParseBulbs(List<string> responses)
        {
            LinkedList<BulbColor> bulbs = new LinkedList<BulbColor>();
            for (int i = 0; i < responses.Count; i++)
            {
                BulbColor bulb = Parse(responses[i]);
                if (bulb.Model == "color")
                {
                    bulb.IsOnline = true;
                    bulbs.AddLast(bulb);
                }
            }
            return bulbs;
        }
        private static BulbColor Parse(string bulbResponse)
        {
            var bulb = new BulbColor();

            if (bulbResponse.Length != 0)
            {
                var response = bulbResponse.Split(new[] { "\r\n" }, StringSplitOptions.None);

                var ipPort = response[4].Substring(LOCATION.Length);
                var temp = ipPort.Split(':');
                bulb.Ip = temp[0];
                bulb.Port = int.Parse(temp[1]);

                bulb.Id = Convert.ToInt32(response[6].Substring(ID.Length), 16);
                bulb.Model = response[7].Substring(MODEL.Length);
                bulb.FwVer = int.Parse(response[8].Substring(FW_VER.Length));
                bulb.IsPowered = response[10].Substring(POWER.Length) == "on" ? true : false;
                bulb.Brightness = int.Parse(response[11].Substring(BRIGHT.Length));
                bulb.ColorMode = int.Parse(response[12].Substring(COLOR_MODE.Length));
                bulb.ColorTemperature = int.Parse(response[13].Substring(CT.Length));
                bulb.Rgb = int.Parse(response[14].Substring(RGB.Length));
                bulb.Hue = int.Parse(response[15].Substring(HUE.Length));
                bulb.Saturation = int.Parse(response[16].Substring(SAT.Length));
                bulb.Name = response[17].Substring(NAME.Length);
                if (bulb.Name == "")
                    bulb.Name = "Bulb";
                if (bulb.Model == "color")
                    return bulb;
                else return new BulbColor();
            }
            return bulb;
        }

        public override int Id { get; set; } = 0;
        public override string Name { get; set; } = "";
        public override string Ip { get; set; } = "";
        public override int Port { get; set; } = 0;
        public override bool IsOnline { get; set; } = false;
        public override bool IsPowered { get; set; } = false;

        public string Model { get; set; } = "";
        public int FwVer { get; set; } = 0;
        public int Brightness { get; set; } = 0;
        public int ColorMode { get; set; } = 0;
        public int ColorTemperature { get; set; } = 0;
        public int Rgb { get; set; } = 0;
        public int Hue { get; set; } = 0;
        public int Saturation { get; set; } = 0;
        public string GetReport()
        {
            const string divider = "\r\n";
            string report =
                LOCATION.ToUpper() + Ip + ":" + Port + divider +
                ID.ToUpper() + Id + divider +
                MODEL.ToUpper() + Model + divider +
                FW_VER.ToUpper() + FwVer + divider +
                POWER.ToUpper() + IsPowered + divider +
                BRIGHT.ToUpper() + Brightness + divider +
                COLOR_MODE.ToUpper() + ColorMode + divider +
                CT.ToUpper() + ColorTemperature + divider +
                RGB.ToUpper() + Rgb + divider +
                HUE.ToUpper() + Hue + divider +
                SAT.ToUpper() + Saturation + divider +
                NAME.ToUpper() + Name + divider;
            return report;
        }

        public Socket AcceptedClient;

        public void TurnMusicModeON(IPAddress localIP, int localPort)
        {
            sendCommand($"{{\"id\":{this.Id},\"method\":\"set_music\",\"params\":[1, \"{localIP}\", {localPort}]}}\r\n");
        }
        public void TurnMusicModeOFF()
        {
            sendCommand($"{{\"id\":{this.Id},\"method\":\"set_music\",\"params\":[0]}}\r\n");
        }
        public void SetColorMode(int value)
        {
            sendCommand($"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[on, \"smooth\", 500, {value}]}}\r\n");
        }
        public void SetNormalLight(int colorTemperature, int brightness)
        {
            sendCommand($"{{\"id\":{this.Id},\"method\":\"set_scene\",\"params\":[\"ct\", {colorTemperature}, {brightness}]}}\r\n");
        }
        private void sendCommand(string command)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(command);

                IAsyncResult result = client.BeginConnect(this.Ip, this.Port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                if (client.Connected)
                {
                    client.EndConnect(result);
                    client.Send(buffer);
                }
            }
        }
        public override string ToString()
        {
            return $"{Name} ID: {Id}";
        }
        ~BulbColor()
        {
            AcceptedClient?.Dispose();
        }
    }
}
