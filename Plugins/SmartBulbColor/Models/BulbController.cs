﻿using System;
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
using System.Windows.Threading;

//MM - MusicMode

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        public override string DeviceType { get; } = "MiBulbColor";
        public override string SSDPMessage { get; } = (
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1982\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: wifi_bulb");
        public LinkedList<Bulb> Bulbs { get; private set; } = new LinkedList<Bulb>();
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
        public LinkedList<Bulb> GetBulbs()
        {
            if(Bulbs.Count != 0)
            {
                return Bulbs;
            }
            else
            {
                ConnectBulbs_MusicMode();
                return Bulbs;
            }
        }
        public delegate void BulbCollectionNotifier();
        public event BulbCollectionNotifier BulbCollectionChanged;
        public ScreenColorAnalyzer ColorAnalyzer;
        SSDPDiscoverer Discoverer;
        Socket TcpServer;
        IPAddress LocalIP;
        int LocalUdpPort;

        readonly Thread ALThread;
        readonly ManualResetEvent ALTrigger;
        readonly Thread BulbsRefresher;
        readonly ManualResetEvent BulbsRefresherTrigger;
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
            BulbsRefresher = new Thread(new ThreadStart(RefreshBulbCollection));
            BulbsRefresher.IsBackground = true;
            BulbsRefresherTrigger = new ManualResetEvent(true);
        }
        public void ConnectBulbs_MusicMode()
        {
            lock(Locker)
            {
                DisconnectBulbs();
                try
                {
                    Bulbs = ParseBulbs(Discoverer.GetDeviceResponses());
                    foreach (var bulb in Bulbs)
                    {
                        bulb.IsOnline = true;
                    }
                    MusicMode_ON();
                }
                catch (Exception NoResponseException)
                {
                    throw NoResponseException;
                }
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
                    throw new Exception(e.Message + "\r\nCan't turn Music Mode ON becouse of some connection problem");
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
            var lostBulbs = new List<Bulb>();
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
                        bulb.AcceptedClient.Send(commandBuffer);
                    }
                    catch (Exception e)
                    {
                        bulb.AcceptedClient.Dispose();
                        bulb.AcceptedClient = null;
                        lostBulbs.Add(bulb);
                        if ((Bulbs.Count - 1) == 0)
                            AmbientLight_OFF();
                    }
                }
                if (lostBulbs.Count != 0)
                {
                    foreach (var lostBulb in lostBulbs)
                    {
                        Bulbs.Remove(lostBulb);
                    }
                }
                previosHue = color.Hue;
                Thread.Sleep(8);
            }
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

                IAsyncResult result = client.BeginConnect(bulb.Ip, bulb.Port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                if (client.Connected)
                {
                    client.EndConnect(result);
                    client.Send(buffer);
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
        private void DisconnectBulbs()
        { 
            if(Bulbs != null && Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    if(bulb.AcceptedClient != null)
                    {
                        bulb.AcceptedClient.Dispose();
                    }
                }
            }
        }
        private void OnBulbConnecionChanged()
        {
            BulbCollectionChanged?.Invoke();
        }
        public void StartBulbsRefreshing()
        {
            if (BulbsRefresher.IsAlive)
            {
                BulbsRefresherTrigger.Set();
            }
            else
            {
                BulbsRefresher.Start();
            }
        }
        void RefreshBulbCollection()
        {
            BulbsRefresherTrigger.WaitOne(Timeout.Infinite);
            while (true)
            {
                Thread.Sleep(5000);
                ConnectBulbs_MusicMode();
                OnBulbConnecionChanged();
            }
        }
        public void Dispose()
        {
            TcpServer.Dispose();
        }
    }
}
