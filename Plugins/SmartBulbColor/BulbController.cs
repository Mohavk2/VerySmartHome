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
using SmartBulbColor.Tools;
using System.Collections.ObjectModel;

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
        public ObservableCollection<Bulb> Bulbs { get; private set; } = new ObservableCollection<Bulb>();
        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }
        public override LinkedList<Device> GetDevices()
        {
            if (Bulbs.Count != 0)
            {
                LinkedList<Device> devices = new LinkedList<Device>(Bulbs);
                return devices;
            }
            else return new LinkedList<Device>();
        }

        public ScreenColorAnalyzer ColorAnalyzer;
        SSDPDiscoverer Discoverer;
        Socket TcpServer;
        IPAddress LocalIP;
        int LocalUdpPort;

        readonly Thread ALThread;
        readonly ManualResetEvent ALTrigger;
        object Locker = new object();
        public bool IsMusicModeON { get; private set; } = false;
        public bool IsAmbientLightON { get; private set;} = false;

        public BulbController()
        {
            Discoverer = new SSDPDiscoverer(SSDPMessage);
            ColorAnalyzer = new ScreenColorAnalyzer();
            LocalIP = Discoverer.GetLocalIP();
            LocalUdpPort = 19446;
            ALThread = new Thread(new ThreadStart(StreamAmbientLightHSL));
            ALThread.IsBackground = true;
            ALTrigger = new ManualResetEvent(true);
        }
        public void ConnectBulbs_MusicMode()
        {
            try
            {
                var foundBulbs = ParseBulbs(Discoverer.GetDeviceResponses());
                var lostBulbs = new LinkedList<Bulb>();
                foreach (var foundBulb in foundBulbs)
                {
                    if (!Bulbs.Contains(foundBulb))
                        Bulbs.Add(foundBulb);
                }
                foreach(var bulb in Bulbs)
                {
                    if (!foundBulbs.Contains(bulb))
                        lostBulbs.AddLast(bulb);
                    else
                        bulb.IsOnline = true;
                }
                foreach (var lostBulb in lostBulbs)
                {
                    Bulbs.Remove(lostBulb);
                }

                MusicMode_ON();
            }
            catch (Exception NoResponseException)
            {
                throw NoResponseException;
            }
        }
        public void MusicMode_ON()
        {
            if (Bulbs.Count != 0)
            {
                try
                {
                    foreach (var bulb in Bulbs)
                    {
                        string commandMessage =
                        $"{{\"id\":{bulb.Id},\"method\":\"set_music\",\"params\":[1, \"{LocalIP}\", {LocalUdpPort}]}}\r\n";
                        sendCommandTo(bulb, commandMessage);
                    }
                    if (TcpServer == null)
                    {
                        TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        TcpServer.Bind(new IPEndPoint(LocalIP, LocalUdpPort));
                        TcpServer.Listen(10);
                    }
                    foreach (var bulb in Bulbs)
                    {
                        if (bulb.AcceptedClient == null)
                        {
                            bulb.AcceptedClient = TcpServer.Accept();
                        }
                        if (!bulb.AcceptedClient.Connected)
                        {
                            bulb.AcceptedClient.Connect(IPAddress.Parse(bulb.Ip), LocalUdpPort);
                        }
                    }
                    IsMusicModeON = true;
                }
                catch (Exception e)
                {
                    throw new Exception("Can't turn Music Mode ON becouse of some connection problem");
                }
            }
            else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
        }
        void MusicMode_OFF()
        {
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    string commandMessage =
                    $"{{\"id\":{bulb.Id},\"method\":\"set_music\",\"params\":[0]}}\r\n";
                    sendCommandTo(bulb, commandMessage);
                }
                IsMusicModeON = false;
            }
            else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
        }
        public void NormalLight_ON()
        {
            AmbientLight_OFF();
            Thread.Sleep(500);
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    var command =
                    $"{{\"id\":{bulb.Id},\"method\":\"set_scene\",\"params\":[\"ct\", {5400}, {100}]}}\r\n";
                    sendCommandTo(bulb, command);
                }
            }
            else
            {
                try
                {
                    ConnectBulbs_MusicMode();
                }
                catch (Exception NoResponseException)
                {
                    throw NoResponseException;
                }
            }
        }
        public void AmbientLight_ON()
        {
            try
            {
                SetColorMode(2);
                if (ALThread.IsAlive)
                {
                    ALTrigger.Set();
                }
                else
                {
                    ALThread.Start();
                }
                IsAmbientLightON = true;
            }
            catch (Exception MusicModeFailedException)
            {
                throw MusicModeFailedException;
            }
        }
        public void AmbientLight_OFF()
        {
            if(IsAmbientLightON)
            {
                ALTrigger.Reset();
                IsAmbientLightON = false;
            }
        }
        void StreamAmbientLightHSL()
        {
            int bulbCounter = Bulbs.Count;
            HSBColor color;
            int previosHue = 0;
            while (true)
            {
                ALTrigger.WaitOne(Timeout.Infinite);

                color = ColorAnalyzer.GetMostCommonColorHSL();

                var bright = color.Brightness;
                var hue = (bright < 1) ? previosHue : color.Hue;
                var sat = color.Saturation;

                foreach (var bulb in Bulbs)
                {
                    string command =
                    $"{{\"id\":{bulb.Id},\"method\":\"set_scene\",\"params\":[\"hsv\", {hue}, {sat}, {bright}]}}\r\n";
                    byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
                    try
                    {
                        if (bulb.IsOnline)
                        bulb.AcceptedClient.Send(commandBuffer);
                    }
                    catch (Exception e)
                    {
                        bulb.IsOnline = false;
                        bulbCounter--;
                        if (bulbCounter == 0)
                            AmbientLight_OFF();
                    }
                }
                previosHue = color.Hue;
                Thread.Sleep(8);
            }
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
        void SetColorMode(int value)
        {
            if (value > 0 & value <= 3)
            {
                if (Bulbs.Count != 0)
                {
                    foreach (var bulb in Bulbs)
                    {
                        string commandMessage = 
                        $"{{\"id\":{bulb.Id},\"method\":\"set_power\",\"params\":[on, \"smooth\", 500, {value}]}}\r\n";
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
                if(bulb.IsOnline)
                {
                    try
                    {
                        client.Connect(new IPEndPoint(IPAddress.Parse(bulb.Ip), bulb.Port));
                        client.Send(buffer);
                    }
                    catch (Exception e)
                    {
                        bulb.IsOnline = false;
                    }
                }
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
