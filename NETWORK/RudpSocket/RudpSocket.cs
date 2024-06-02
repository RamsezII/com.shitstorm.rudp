using _UTIL_;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public partial class RudpSocket : Socket, IDisposable
    {
        public static readonly Encoding UTF8 = Encoding.UTF8;

        public readonly byte[] PAQUET_BUFFER = new byte[Util_rudp.PAQUET_SIZE];
        public readonly byte[] ACK_BUFFER = new byte[RudpHeader.HEADER_length];

        public readonly ushort localPort;
        readonly EndPoint endIP_any;
        public readonly IPEndPoint endIP_loopback, endIP_LAN;

        public readonly ThreadSafe<bool> disposed = new();

        public readonly RudpConnection selfConn;
        public EveComm eveComm;

        public override string ToString() => $"(socket {endIP_LAN})";

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in ushort port = 0) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            recStream_u = new(PAQUET_BUFFER);
            recReader_u = new(recStream_u, UTF8, false);
            recDataStream = new();
            recDataReader = new(recDataStream, UTF8, false);

            ExclusiveAddressUse = false;
            if (port != 0)
                Bind(new IPEndPoint(IPAddress.Any, port));

            SendTo(PAQUET_BUFFER, 0, 0, SocketFlags.None, Util_rudp.END_LOOPBACK);
            endIP_any = LocalEndPoint;
            localPort = (ushort)((IPEndPoint)endIP_any).Port;
            endIP_loopback = new(IPAddress.Loopback, localPort);
            endIP_LAN = new(Util_rudp.localIP, localPort);
            Debug.Log($"opened UDP: {this}".ToSubLog());

            selfConn = ToConnection((IPEndPoint)endIP_any);
            lock (connections)
                connections[endIP_loopback] = connections[endIP_LAN] = selfConn;

            eveComm = new(ToConnection(Util_rudp.END_RUDP));

            BeginReceive();
        }

        //----------------------------------------------------------------------------------------------------------

        public new void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;

            skipNextSocketException.Value = true;

            Debug.Log($"closing socket: {this}".ToSubLog());
            base.Dispose();
            Close();

            recStream_u.Dispose();
            recReader_u.Dispose();
            recDataStream.Dispose();
            recDataReader.Dispose();
            eveComm.Dispose();

            if (connections.Count > 0)
            {
                foreach (RudpConnection conn in connections.Values)
                    conn.Dispose();
                lock (connections)
                    connections.Clear();
            }
        }
    }
}