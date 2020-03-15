using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VerySmartHome.Interfaces;

namespace VerySmartHome.MainController
{
    public delegate void DeviceFoundEventHandler(IDevice foundDevice);
    public delegate void DeviceLostEventHandler(IDevice lostDevice);

    public abstract class DeviceDiscoverer
    {
        protected List<IDevice> RelevantDevices = new List<IDevice>();

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

            RefreshingThread = new Thread(new ThreadStart(DiscoveringDevices));
            RefreshingThread.IsBackground = true;
            RefreshingTrigger = new ManualResetEvent(true);

            Device.NoResponseFromDevice += OnNoResponseFromDevice;
        }
        public DeviceDiscoverer(string message, string ip, int port) : this(message)
        {
            this.MulticastIP = ip;
            this.MulticastPort = port;
        }
        public List<IDevice> DiscoverDevices()
        {
            var responses = GetDeviceResponses();
            return CreateDevices(responses);
        }
        public virtual List<string> GetDeviceResponses()
        {
            lock (Locker)
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

                    if (responders.Count == 0 | !(responders.Contains(responder)))
                    {
                        responders.Add(responder);
                        responses.Add(Encoding.UTF8.GetString(response));
                    }
                }
                searcher.Dispose();
                return responses;
            }
        }
        protected virtual List<IDevice> CreateDevices(List<string> responses)
        {
            List<IDevice> devices = new List<IDevice>();
            for (int i = 0; i < responses.Count; i++)
            {
                IDevice device = CreateDevice(responses[i]);
                devices.Add(device);
            }
            return devices;
        }
        protected abstract IDevice CreateDevice(string response);
        private void DiscoveringDevices()
        {
            while (true)
            {
                RefreshingTrigger.WaitOne(Timeout.Infinite);

                var foundDevices = DiscoverDevices();
                RefreshDevicesOnline(foundDevices);
                Thread.Sleep(RefreshTimeout);
            }
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
        public void StopDiscover()
        {
            RefreshingTrigger.Reset();
        }
        private void RefreshDevicesOnline(List<IDevice> foundDevices)
        {
            RemoveLostDevices(foundDevices);
            AddNewDevices(foundDevices);
        }
        private void RemoveLostDevices(List<IDevice> foundDevices)
        {
            if(foundDevices == null || foundDevices.Count == 0)
            {
                foreach(var device in RelevantDevices)
                {
                    OnDeviceLost(device);
                }
                RelevantDevices.Clear();
            }
            if (RelevantDevices != null && RelevantDevices.Count != 0)
            {
                for (int i = 0; i < RelevantDevices.Count; i++)
                {
                    bool containsLost = true;
                    for (int j = 0; j < foundDevices.Count; j++)
                    {
                        if (foundDevices[j].GetId() == RelevantDevices[i].GetId())
                            containsLost = false;
                    }
                    if (containsLost)
                    {
                        var lostDevice = RelevantDevices[i];
                        OnDeviceLost(lostDevice);
                    }
                }
            }
        }
        private void AddNewDevices(List<IDevice> foundDevices)
        {
            if (foundDevices != null && foundDevices.Count != 0)
            {
                for (int i = 0; i < foundDevices.Count; i++)
                {
                    bool alreadyExists = false;
                    for (int j = 0; j < RelevantDevices.Count; j++)
                    {
                        if (foundDevices[i].GetId() == RelevantDevices[j].GetId())
                            alreadyExists = true;
                    }
                    if (alreadyExists == false)
                    {
                        var foundDevice = foundDevices[i];
                        OnDeviceFound(foundDevice);
                    }
                }
            }
        }
        private void OnDeviceFound(IDevice foundDevice)
        {
            lock(Locker)
            {
                DeviceFound?.Invoke(foundDevice);
                RelevantDevices.Add(foundDevice);
            }
        }
        private void OnDeviceLost(IDevice lostDevice)
        {
            lock(Locker)
            {
                DeviceLost?.Invoke(lostDevice);
                RelevantDevices.Remove(lostDevice);
                lostDevice?.Dispose();
            }
        }
        private void OnNoResponseFromDevice(IDevice device)
        {
            OnDeviceLost(device);
        }
    }
}
