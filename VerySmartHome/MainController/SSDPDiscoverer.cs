using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    class SSDPDiscoverer
    {
        public string SearchMessage { get; set; }
        public string MulticastIP { get; set; } = "239.255.255.250";
        public int MulticastPort { get; set; } = 1982;
        public SSDPDiscoverer(string message)
        {
            this.SearchMessage = message;
        }
        public SSDPDiscoverer(string message, string ip, int port) : this(message)
        {
            this.MulticastIP = ip;
            this.MulticastPort = port;
        }
        public /*IPEndPoint[]*/ string GetLANDeviceEndPoints()
        {
            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var localEP = new IPEndPoint(GetLocalIP(), 60000);
            var multicastEP = new IPEndPoint(IPAddress.Parse(MulticastIP), MulticastPort);

            udpSocket.Bind(localEP);
            udpSocket.SendTo(Encoding.UTF8.GetBytes(SearchMessage), multicastEP);

            byte[] recData = new byte[64000];
            udpSocket.ReceiveTimeout = 5000;
            udpSocket.Receive(recData);
            udpSocket.Dispose();
            if (recData.Length != 0)
            {
                var answer = Encoding.UTF8.GetString(recData);
                return answer;
                /*IPEndPoint[] endPoints = ParseEndPoints(answer);
                return endPoints;*/
            }
            else
            {
                throw new Exception("No Devices Found Exception");
            }
        }
        /*
        private IPEndPoint[] ParseEndPoints(string answer)
        {
            IPEndPoint[] endPoints = null;
            string substring = 
            if(endPoints != null)
            {
                return endPoints;
            }
            else
            {
                throw new Exception("Invalid device answers exception. No entry points found!");
            }
        }
        */
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
