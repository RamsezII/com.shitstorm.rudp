using _UTIL_;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public partial class RudpConnection : Disposable
    {
#if UNITY_EDITOR
        [Header("~@ Conn @~")]
        [SerializeField] bool _initiated;
        [SerializeField] bool _no_relay;
#endif
        public readonly bool no_relay;

        public readonly RudpSocket socket;
        public IPEndPoint endPoint, localEnd, publicEnd;

        [Range(0, uint.MaxValue)] public uint u32_ip;
        [Range(0, ushort.MaxValue)] public ushort u16_port;

        public readonly ThreadSafe_struct<double>
            lastSend = new(),
            lastReceive = new();

        public readonly RudpChannel
            ftpChannel,
            statesChannel,
            fluxChannel,
            audioChannel;

        public readonly MemoryStream states_recStream;
        public readonly BinaryReader states_recReader;

        //----------------------------------------------------------------------------------------------------------

        internal RudpConnection(in RudpSocket socket, in IPEndPoint endPoint, in bool no_relay) : base($"rudp_conn({socket.localPort}->{endPoint})")
        {
            this.socket = socket;
            this.endPoint = endPoint;
            this.no_relay = no_relay;

            u32_ip = (uint)endPoint.Address.Address;
            u16_port = (ushort)endPoint.Port;

#if UNITY_EDITOR
            _initiated = true;
            _no_relay = no_relay;
#endif

            ftpChannel = new("rudp_files", this, RudpHeaderM.Files);
            statesChannel = new("rudp_states", this, RudpHeaderM.States);
            fluxChannel = new("rudp_flux", this, RudpHeaderM.Flux);
            audioChannel = new("rudp_audio", this, RudpHeaderM.Audio);

            states_recStream = new();
            states_recReader = new(states_recStream, Util_rudp.ENCODING, false);
        }

        //----------------------------------------------------------------------------------------------------------

        public void WriteConnection(in BinaryWriter writer)
        {
            writer.Write(publicEnd);
            writer.Write(localEnd);
        }

        public void AppendStatus(in StringBuilder log)
        {
            lock (this)
                statesChannel.AppendStatesStatus(log);
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();

            ftpChannel.Dispose();
            statesChannel.Dispose();
            fluxChannel.Dispose();
            audioChannel.Dispose();

            states_recStream.Dispose();
            states_recReader.Dispose();
        }
    }
}