using _UTIL_;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket : Socket, IDisposable
    {
        public const ushort
            PAQUET_SIZE = 1472,
            DATA_SIZE = PAQUET_SIZE - RudpHeader.HEADER_length;

        public static readonly Encoding UTF8 = Encoding.UTF8;

        public readonly byte[] PAQUET_BUFFER = new byte[PAQUET_SIZE];
        public readonly byte[] ACK_BUFFER = new byte[RudpHeader.HEADER_length];

        public readonly ushort localPort;
        readonly EndPoint endIP_any;
        public readonly IPEndPoint endIP_loopback, endIP_LAN;

        public readonly ThreadSafe<bool> disposed = new();

        public static bool
            logEmptyPaquets = false,
            logAllPaquets = false;

        public readonly RudpConnection selfConn;
        public readonly EveClient eveClient;

        public override string ToString() => $"(socket {endIP_LAN})";

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in Action<RudpConnection, RudpHeaderM, BinaryReader> onDirectRead, in ushort port = 0) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            directStream = new(PAQUET_BUFFER);
            directReader = new(directStream, UTF8, true);
            bufferStream = new();
            bufferReader = new(bufferStream, UTF8, true);

            this.onDirectRead.Value = onDirectRead;

            ExclusiveAddressUse = false;
            if (port != 0)
                Bind(new IPEndPoint(IPAddress.Any, port));

            SendTo(PAQUET_BUFFER, 0, 0, SocketFlags.None, Util.END_LOOPBACK);
            endIP_any = LocalEndPoint;
            localPort = (ushort)((IPEndPoint)endIP_any).Port;
            endIP_loopback = new(IPAddress.Loopback, localPort);
            endIP_LAN = new(Util.localIP, localPort);
            Debug.Log($"opened UDP: {this}".ToSubLog());

            selfConn = ToConnection((IPEndPoint)endIP_any);
            lock (connections)
                connections[endIP_loopback] = connections[endIP_LAN] = selfConn;

            eveClient = new(ToConnection(Util.END_RUDP));

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

            directStream.Dispose();
            directReader.Dispose();
            bufferStream.Dispose();
            bufferReader.Dispose();

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