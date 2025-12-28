using _ARK_;
using _UTIL_;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public partial class RudpSocket : Socket, IDisposable
    {
        public readonly byte[] recBuffer_u = new byte[Util_rudp.PAQUET_SIZE_BIG];
        public readonly byte[] ACK_BUFFER = new byte[RudpHeader.HEADLEN_B];

        public readonly bool use_relay;
        public readonly ushort localPort;
        readonly EndPoint endIP_any;
        public readonly IPEndPoint endIP_loopback;

        public readonly ThreadSafe_struct<bool> disposed = new();

        public readonly RudpConnection selfConn;
        public RudpConnection relayConn;
        public EveComm eveComm;

        public readonly MemoryStream recStream_u, flux_recStream;
        public readonly BinaryReader recReader_u, flux_recReader;
        public bool HasNext() => recStream_u.Position < recLength_u;

#if UNITY_EDITOR
        [SerializeField] bool _use_relay;
        [SerializeField] RudpConnection _selfConn;
#endif
        public override string ToString() => $"sock{{{localPort}}}";

        //----------------------------------------------------------------------------------------------------------

        ~RudpSocket() => Debug.Log($"~{this}");

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            r_settings.Reset();
            r_settings.ForcedValue();
            LoadSettings(true);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            NUCLEOR.delegates.OnApplicationFocus += () => LoadSettings(false);
            NUCLEOR.delegates.OnApplicationUnfocus += () => h_settings.SaveStaticJSon(false);
        }

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in bool use_relay, in ushort port = 0) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            this.use_relay = use_relay;

            recStream_u = new(recBuffer_u);
            recReader_u = new(recStream_u, Util_rudp.ENCODING, false);
            flux_recStream = new();
            flux_recReader = new(flux_recStream, Util_rudp.ENCODING, false);

            ExclusiveAddressUse = false;
            if (port != 0)
                Bind(new IPEndPoint(IPAddress.Any, port));

            SendTo(recBuffer_u, 0, 0, SocketFlags.None, Util_rudp.END_LOOPBACK);
            endIP_any = LocalEndPoint;
            localPort = (ushort)((IPEndPoint)endIP_any).Port;
            endIP_loopback = new(IPAddress.Loopback, localPort);

            selfConn = ToConnection((IPEndPoint)endIP_any, true, out _);
            selfConn.localEnd = new(Util_rudp.localIP, localPort);
            lock (conns_dic)
                conns_dic[endIP_loopback] = conns_dic[selfConn.localEnd] = selfConn;

#if UNITY_EDITOR
            _use_relay = use_relay;
            _selfConn = selfConn;
#endif

            eveComm = new(ToConnection(Util_rudp.END_ARMA, true, out _));
            relayConn = ToConnection(Util_rudp.END_RELAY, true, out _);

            eveComm.conn.keepAlive = true;

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
            flux_recStream.Dispose();
            flux_recReader.Dispose();
            eveComm.Dispose();

            lock (conns_set)
                if (conns_set.Count > 0)
                {
                    foreach (RudpConnection conn in conns_set)
                        conn.Dispose();
                    conns_set.Clear();
                }

            lock (conns_dic)
                if (conns_dic.Count > 0)
                    conns_dic.Clear();
        }
    }
}