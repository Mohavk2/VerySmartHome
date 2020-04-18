using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public delegate void DeviceFoundEventHandler(Device foundDevice);

    public abstract class DeviceDiscoverer
    {
        List<int> RelevantIds = new List<int>();

        int RefreshTimeout = 3000;
        readonly Thread RefreshingThread;
        readonly ManualResetEvent RefreshingTrigger;
        protected Object Locker = new Object();

        public static event DeviceFoundEventHandler DeviceFound;

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
        public void StopDiscover()
        {
            RefreshingTrigger.Reset();
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
            lock (Locker)
            {
                var responses = GetResponses();
                foreach (var response in responses)
                {
                    int id = ParseId(response);

                    if(id != -1)//if success
                    {
                        if(!RelevantIds.Contains(id))
                        {
                            RelevantIds.Add(id);
                            OnDeviceFound(CreateDevice(response));
                        }
                    }
                }
            }
        }
        public void FindDevices()
        {
            RefreshDevices();
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

            Thread.Sleep(1500); //to give a time to devices for responding
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
        /// <summary>
        /// Returns Id if exist, else returns -1
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected int ParseId(string response)
        {
            var targetStartsWith = "id: ";

            string[] properties = response.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var property in properties)
            {
                if (property.Contains(targetStartsWith))
                {
                    return Convert.ToInt32(property.Substring(targetStartsWith.Length), 16);
                }
            }
            return -1;
        }
        protected abstract Device CreateDevice(string response);

        private void OnDeviceFound(Device foundDevice)
        {
            DeviceFound?.Invoke(foundDevice);
        }
    }
}
