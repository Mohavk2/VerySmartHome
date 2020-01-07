using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;
using VerySmartHome.Tools;

//MM - MusicMode

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

        SSDPDiscoverer Discoverer;
        Socket UdpServer;
        IPAddress LocalIP;
        int LocalUdpPort;

        public BulbController()
        {
            Discoverer = new SSDPDiscoverer(SSDPMessage);
            LocalIP = Discoverer.GetLocalIP();
            LocalUdpPort = 19446;
        }
        public void DiscoverForBulbs()
        {
            try
            {
                Bulbs = ParseBulbs(Discoverer.GetDeviceResponses());
                DeviceCount = Bulbs.Count;
            }
            catch (Exception NoDeviceException)
            {
                throw NoDeviceException = new Exception("No device found!!!");
            }
        }
        public void TurnOnAmbientLight_All()
        {
            try
            {
                TurnOffMusicMode_All();
                TurnOnMusicMode_All();
                SetColorMode_All(2);
                Thread thread = new Thread(new ThreadStart(StreamAmbientLight));
                if (!thread.IsAlive)
                {
                    thread.Start();
                }
                else thread.Abort();
            }
            catch(Exception MusicModeFailedException)
            {
                throw MusicModeFailedException;
            }
        }
        void StreamAmbientLight()
        {
            UdpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UdpServer.Bind(new IPEndPoint(LocalIP, LocalUdpPort));
            UdpServer.Listen(10);
            foreach (var bulb in Bulbs)
            {
                bulb.AcceptedClient = UdpServer.Accept();
                if (!bulb.AcceptedClient.Connected)
                {
                    bulb.AcceptedClient.Connect(IPAddress.Parse(bulb.Ip), LocalUdpPort);
                }
            }
            ScreenColorAnalyzer analyzer = new ScreenColorAnalyzer();
            Color color;

            while (true)
            {
                Thread.Sleep(10);
                color = analyzer.GetAvgScreenColor();
                int colorDecimal = RGBToDecimal(color);

                foreach (var bulb in Bulbs)
                {
                    string commandMessage = $"{{\"id\":{bulb.Id},\"method\":\"set_rgb\",\"params\":[{colorDecimal}]}}\r\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(commandMessage);
                    bulb.AcceptedClient.Send(buffer);
                }
            }
        }
        public void TurnOnMusicMode_All()
        {
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    string commandMessage = $"{{\"id\":{bulb.Id},\"method\":\"set_music\",\"params\":[1, \"{LocalIP}\", {LocalUdpPort}]}}\r\n";
                    sendCommandTo(bulb, commandMessage);
                }
            }
            else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
        }
        void TurnOnMusicMode_Bulbs()
        {

        }
        void TurnOffMusicMode_All()
        {
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    string commandMessage = $"{{\"id\":{bulb.Id},\"method\":\"set_music\",\"params\":[0]}}\r\n";
                    sendCommandTo(bulb, commandMessage);
                }
            }
            else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
        }
        void TurnOffMusicMode_Bulbs()
        {

        }
        void ChangeColor_Bulb()
        {

        }
        int RGBToDecimal(Color rgbColor)
        {
            int color = (rgbColor.R * 65536) + (rgbColor.G * 256) + rgbColor.B;
            return color;
        }
        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        void SetColorMode_All(int value)
        {
            if (value > 0 & value <= 3)
            {
                if (Bulbs.Count != 0)
                {
                    foreach (var bulb in Bulbs)
                    {
                        string commandMessage = $"{{\"id\":{bulb.Id},\"method\":\"set_power\",\"params\":[on, \"smooth\", 500, {value}]}}\r\n";
                        sendCommandTo(bulb, commandMessage);
                    }
                }
                else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
            }
        }

        void sendCommandTo(Bulb bulb, string command)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                
                byte[] buffer = Encoding.UTF8.GetBytes(command);
                client.Connect(new IPEndPoint(IPAddress.Parse(bulb.Ip), bulb.Port));
                client.Send(buffer);
                client.ReceiveTimeout = 5000;
                while (client.Available > 0)
                {
                    byte[] recBuffer = new byte[128];
                    client.Receive(recBuffer);
                    var response = Encoding.UTF8.GetString(recBuffer);
                    if (!response.Contains("\"result\":[\"ok\"]"))
                        throw new Exception($"Failed to turn on Music Mode for {bulb.Id}");
                }
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
            for (int i = 0; i < responses.Count; i++)
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
