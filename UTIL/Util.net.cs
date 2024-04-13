using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public static partial class Util
    {
        public const byte VERSION = 0;
        public const string DOMAIN_3VE = "www.shitstorm.ovh";
        public const ushort PORT_RUDP = 12345;
        public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");
        public static readonly IPEndPoint END_RUDP = new(IP_3VE, PORT_RUDP);
        public static readonly IPEndPoint END_LOOPBACK = new(IPAddress.Loopback, PORT_RUDP);
        public static IPAddress localIP;

        //----------------------------------------------------------------------------------------------------------

        static void InitNet()
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(new IPEndPoint(IPAddress.Parse("8.8.8.8"), 1234));
            localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
        }
    }
}