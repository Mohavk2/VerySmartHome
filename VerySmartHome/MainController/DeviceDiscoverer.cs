using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VerySmartHome.MainController
{
    public delegate void DeviceFoundEventHandler(Device foundDevice);
    public delegate void DeviceLostEventHandler(Device lostDevice);

    //TO DO add passive listening of Multicast EndPoint to recognize if device is steel online
    public abstract class DeviceDiscoverer
    {
        private List<Device> Relevant = new List<Device>();
        private List<Device> LostByReport = new List<Device>();

        int RefreshTimeout = 3000;
        readonly Thread RefreshingThread;
        readonly ManualResetEvent RefreshingTrigger;
        protected Object Locker = new Object();

        public static event DeviceFoundEventHandler DeviceFound;
        public static event DeviceLostEventHandler DeviceLost;

        public string SearchMessage { get; set; }
        public string MulticastIP { get; set; } = "239.255.255.250";
        public int MulticastPort { get; set; } = 1982;
        private int LocalPort { get; set; } = 65212;

        public static IPAddress GetLocalIP()
        {
            IPAddress localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return localIP = endPoint.Address;
            }
        }
        public DeviceDiscoverer(string message)
        {
            this.SearchMessage = message;

            RefreshingThread = new Thread(new ThreadStart(Discovering));
            RefreshingThread.IsBackground = true;
            RefreshingTrigger = new ManualResetEvent(true);

            Device.NoResponseFromDevice += OnNoResponseFromDevice;
        }
        public DeviceDiscoverer(string message, string ip, int port) : this(message)
        {
            this.MulticastIP = ip;
            this.MulticastPort = port;
        }
        public void StartDiscover()
        {
            if (RefreshingThread.IsAlive)
            {
                RefreshingTrigger.Set();
            }
            else
            {
                RefreshingThread.Start();
            }
        }
        private void Discovering()
        {
            while (true)
            {
                RefreshingTrigger.WaitOne(Timeout.Infinite);
                RefreshDevices();
                Thread.Sleep(RefreshTimeout);
            }
        }
        public void RefreshDevices()
        {
            lock(Locker)
            {
                RemoveLostByReport();
                var responses = GetResponses();
                var foundDevices = CreateDevices(responses);
                RefreshRelevant(foundDevices);
            }
        }
        public List<Device> FindDevices()
        {
            RefreshDevices();
            return new List<Device>(Relevant);
        }
        public virtual List<string> GetResponses()
        {
            var searcher = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var multicast = new IPEndPoint(IPAddress.Parse(MulticastIP), MulticastPort);
            var responder = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            var responders = new List<EndPoint>();
            var response = new byte[1024];
            var responses = new List<string>();

            searcher.Bind(new IPEndPoint(GetLocalIP(), LocalPort));
            searcher.SendTo(Encoding.UTF8.GetBytes(SearchMessage), multicast);

            Thread.Sleep(1000); //to give a time to devices for responding
            searcher.ReceiveTimeout = 2000;
            while (searcher.Available > 0)
            {
                searcher.ReceiveFrom(response, ref responder);

                if (responders.Count == 0 || !(responders.Contains(responder)))
                {
                    responders.Add(responder);
                    responses.Add(Encoding.UTF8.GetString(response));
                }
            }
            searcher.Dispose();
            return responses;
        }
        protected virtual List<Device> CreateDevices(List<string> responses)
        {
            List<Device> devices = new List<Device>();
            for (int i = 0; i < responses.Count; i++)
            {
                Device device = CreateDevice(responses[i]);
                devices.Add(device);
            }
            return devices;
        }
        protected abstract Device CreateDevice(string response);
        public void StopDiscover()
        {
            RefreshingTrigger.Reset();
        }
        private void RefreshRelevant(List<Device> discovered)
        {
            var lost = PickLost(discovered);
            RemoveLost(lost);
            var found = PickFound(discovered);
            AddFound(found);
        }
        private List<Device> PickLost(List<Device> discovered)
        {
            List<Device> lost = new List<Device>();

            if (discovered.Count == 0)
            {
                foreach(var device in Relevant)
                {
                    lost.Add(device);
                }
            }
            else if (Relevant.Count != 0)
            {
                for (int i = 0; i < Relevant.Count; i++)
                {
                    bool containsLost = true;
                    for (int j = 0; j < discovered.Count; j++)
                    {
                        if (discovered[j].GetId() == Relevant[i].GetId())
                            containsLost = false;
                    }
                    if (containsLost)
                    {
                        lost.Add(Relevant[i]);
                    }
                }
            }
            return lost;
        }
        private List<Device> PickFound(List<Device> discovered)
        {
            List<Device> found = new List<Device>();

            if (discovered.Count != 0)
            {
                for (int i = 0; i < discovered.Count; i++)
                {
                    bool alreadyExists = false;
                    for (int j = 0; j < Relevant.Count; j++)
                    {
                        if (discovered[i].GetId() == Relevant[j].GetId())
                            alreadyExists = true;
                    }
                    if (alreadyExists == false)
                    {
                        found.Add(discovered[i]);
                    }
                }
            }
            return found;
        }
        void RemoveLost(List<Device> lost)
        {            
            foreach (var device in lost)
            {
                Relevant.Remove(device);
                OnDeviceLost(device);
            }
        }
        void AddFound(List<Device> found)
        {
            foreach (var device in found)
            {
                Relevant.Add(device);
                OnDeviceFound(device);
            }
        }
        void RemoveLostByReport()
        {
            lock(Locker)
            {
                if (LostByReport.Count != 0 && Relevant.Count != 0)
                {
                    foreach(var device in LostByReport)
                    {
                        Relevant.Remove(device);
                    }
                }
            }
        }
        private void OnDeviceFound(Device foundDevice)
        {
            lock(Locker)
            {
                DeviceFound?.Invoke(foundDevice);
            }
        }
        private void OnDeviceLost(Device lostDevice)
        {
            lock(Locker)
            {
                DeviceLost?.Invoke(lostDevice);
                lostDevice.Disconnect();
            }
        }
        private void OnNoResponseFromDevice(Device device)
        {
            LostByReport.Add(device);
            OnDeviceLost(device);
        }
    }
}
