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
    class SSDPDiscoverer
    {
        public string SearchMessage { get; set; }
        public string MulticastIP { get; set; } = "239.255.255.250";
        public int MulticastPort { get; set; } = 1982;
        private string CollectiveResponse { get; set; } = "";
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
            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var localEP = new IPEndPoint(GetLocalIP(), 60000);
            var multicastEP = new IPEndPoint(IPAddress.Parse(MulticastIP), MulticastPort);

            udpSocket.Bind(localEP);
            udpSocket.SendTo(Encoding.UTF8.GetBytes(SearchMessage), multicastEP);
            Thread.Sleep(1000);
            byte[] recData = new byte[1024];
            udpSocket.ReceiveTimeout = 5000;
            List<string>collectiveResponse = new List<string>();
            while (udpSocket.Available > 0)
            {
                udpSocket.Receive(recData);
                collectiveResponse.Add(Encoding.UTF8.GetString(recData));
            }
            udpSocket.Dispose();
            if (collectiveResponse.Count != 0)
            {
                return collectiveResponse;
            }
            else
            {
                throw new Exception("Devices no response exception");
            }           
        }
        public IPAddress GetLocalIP()
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
}
