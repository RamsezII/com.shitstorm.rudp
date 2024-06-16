using _UTIL_;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public partial class RudpSocket : Socket, IDisposable
    {
        public readonly byte[] recBuffer_u = new byte[Util_rudp.PAQUET_SIZE];
        public readonly byte[] ACK_BUFFER = new byte[RudpHeader.HEADER_length];

        public readonly ushort localPort;
        readonly EndPoint endIP_any;
        public readonly IPEndPoint endIP_loopback;

        public readonly ThreadSafe<bool> disposed = new();

        public readonly RudpConnection selfConn;
        public EveComm eveComm;

        public readonly MemoryStream recStream_u, states_recStream, flux_recStream;
        public readonly BinaryReader recReader_u, states_recReader, flux_recReader;
        public bool HasNext() => recStream_u.Position < recLength_u;

#if UNITY_EDITOR
        [SerializeField] RudpConnection _selfConn;
#endif

        public override string ToString() => $"(socket {selfConn.endPoint.Port})";

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in ushort port = 0) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            recStream_u = new(recBuffer_u);
            recReader_u = new(recStream_u, Util_rudp.ENCODING, false);
            states_recStream = new();
            states_recReader = new(states_recStream, Util_rudp.ENCODING, false);
            flux_recStream = new();
            flux_recReader = new(flux_recStream, Util_rudp.ENCODING, false);

            ExclusiveAddressUse = false;
            if (port != 0)
                Bind(new IPEndPoint(IPAddress.Any, port));

            SendTo(recBuffer_u, 0, 0, SocketFlags.None, Util_rudp.END_LOOPBACK);
            endIP_any = LocalEndPoint;
            localPort = (ushort)((IPEndPoint)endIP_any).Port;
            endIP_loopback = new(IPAddress.Loopback, localPort);

            selfConn = ToConnection((IPEndPoint)endIP_any);
            selfConn.localEnd = new(Util_rudp.localIP, localPort);
            lock (conns_dic)
                conns_dic[endIP_loopback] = conns_dic[selfConn.localEnd] = selfConn;

#if UNITY_EDITOR
            _selfConn = selfConn;
#endif

            eveComm = new(ToConnection(Util_rudp.END_RUDP));


            ebroadcast = conns_set.Where(conn => conn != selfConn && conn != eveComm.conn);

            Debug.Log($"opened UDP: {this}".ToSubLog());
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
            states_recStream.Dispose();
            states_recReader.Dispose();
            flux_recStream.Dispose();
            flux_recReader.Dispose();
            eveComm.Dispose();

            if (conns_set.Count > 0)
            {
                foreach (RudpConnection conn in conns_set)
                    conn.Dispose();
                lock (conns_set)
                    conns_set.Clear();
            }

            if (conns_dic.Count > 0)
                lock (conns_dic)
                    conns_dic.Clear();
        }
    }
}