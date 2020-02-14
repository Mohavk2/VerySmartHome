using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    public class SSDPDiscoverer
    {
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

        public string SearchMessage { get; set; }
        public string MulticastIP { get; set; } = "239.255.255.250";
        public int MulticastPort { get; set; } = 1982;
        private int LocalPort { get; set; } = 65212;
        public SSDPDiscoverer(string message)
        {
            this.SearchMessage = message;
        }
        public SSDPDiscoverer(string message, string ip, int port) : this(message)
        {
            this.MulticastIP = ip;
            this.MulticastPort = port;
        }
        public List<string> GetDeviceResponses()
        {
            var searcher = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var multicast = new IPEndPoint(IPAddress.Parse(MulticastIP), MulticastPort);
            var responder = (EndPoint) new IPEndPoint(IPAddress.Any, 0);
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
}
