using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommonLibrary
{
    public delegate void HTTPResponsesReceivedHandler(Dictionary<string, List<string>> variousResponses);

    public class HsdpDiscoverer
    {
        static List<DiscovererClient> Clients = new List<DiscovererClient>();

        public static string MulticastIP { get; set; } = "239.255.255.250";
        static int LocalPort { get; set; } = 65212;
        static int RefreshTimeout = 3000;

        static readonly Thread RefreshingThread;
        static readonly ManualResetEvent RefreshingTrigger;
        static Object Locker = new Object();

        public static event HTTPResponsesReceivedHandler HTTPResponsesFound;
        private static void OnHTTPResponsesFound(Dictionary<string, List<string>> variousResponses)
        {
            HTTPResponsesFound?.Invoke(variousResponses);
        }

        public static IPAddress GetLocalIP()
        {
            lock (Locker)
            {
                IPAddress localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return localIP = endPoint.Address;
                }
            }
        }
        static HsdpDiscoverer()
        {
            RefreshingThread = new Thread(new ThreadStart(Discovering));
            RefreshingThread.IsBackground = true;
            RefreshingTrigger = new ManualResetEvent(true);
        }

        static void StartDiscovering()
        {
            if (Clients.Count != 0)
            {
                if ((RefreshingThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                {
                    RefreshingThread.Start();
                }
                else
                {
                    RefreshingTrigger.Set();
                }
            }
        }
        static void StopDiscovering()
        {
            RefreshingTrigger.Reset();
        }
        static void Discovering()
        {
            while (true && Clients.Count != 0)
            {
                RefreshingTrigger.WaitOne(Timeout.Infinite);
                SearchBySSDP();
                Thread.Sleep(RefreshTimeout);
            }
            StopDiscovering();
        }
        static void SearchBySSDP()
        {
            lock (Locker)
            {
                Dictionary<string, List<string>> variousResponses = new Dictionary<string, List<string>>();
                foreach (var client in Clients)
                {
                    variousResponses.Add(client.DeviceType, GetDeviceResponsesBySSDP(client));
                }
                OnHTTPResponsesFound(variousResponses);
            }
        }
        static List<string> GetDeviceResponsesBySSDP(DiscovererClient client)
        {
            using (Socket searcher = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                var multicast = new IPEndPoint(IPAddress.Parse(MulticastIP), client.Port);
                var responder = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                var responders = new List<EndPoint>();
                var response = new byte[1024];
                var responses = new List<string>();

                searcher.Bind(new IPEndPoint(GetLocalIP(), LocalPort));
                searcher.SendTo(Encoding.UTF8.GetBytes(client.SsdpMessage), multicast);

                Thread.Sleep(1500); //to give a time to devices for responding
                searcher.ReceiveTimeout = 2000;
                while (searcher.Available > 0)
                {
                    searcher.ReceiveFrom(response, ref responder);

                    if (responders.Count == 0 || !(responders.Contains(responder)))//to avoid duplicating messages
                    {
                        responders.Add(responder);
                        responses.Add(Encoding.UTF8.GetString(response));
                    }
                }
                return new List<string>(responses);
            }
        }
        struct DiscovererClient
        {
            public string SsdpMessage { get; }
            public string DeviceType { get; }
            public int Port { get; }

            public DiscovererClient(string ssdpMessage, string deviceType, int port)
            {
                SsdpMessage = ssdpMessage;
                DeviceType = deviceType;
                Port = port;
            }
        }
        ///////////////////*Client instance Part*//////////////////
        public delegate void ResponsesReceivedHandler(List<string> responses);
        public event ResponsesReceivedHandler ResponsesReceived;

        DiscovererClient Client;

        List<int> IdsToIgnore = new List<int>();
        public bool IgnoreAlreadyFoundIds = true;

        public HsdpDiscoverer(HsdpSearchingAtributes Atributes)
        {
            Client = new DiscovererClient(Atributes.SsdpMessage, Atributes.DeviceType, Atributes.MulticastPort);
            Clients.Add(Client);

            HTTPResponsesFound += NotifySubscriber;
        }
        void NotifySubscriber(Dictionary<string, List<string>> variousResponses)
        {
            if (variousResponses.ContainsKey(Client.DeviceType))
            {
                ResponsesReceived?.Invoke(variousResponses[Client.DeviceType]);
            }        
        }
        public void StartDiscover()
        {
            lock (Locker)
            {
                if (!Clients.Contains(Client))
                {
                    Clients.Add(Client);
                }
                StartDiscovering();
            }
        }
        public void StopDiscover()
        {
            lock (Locker)
            {
                Clients.Remove(Client);
            }
        }
    }
}
