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
        [SerializeField] bool _is_relayed;
        [SerializeField] bool _no_relay;
#endif
        public readonly bool is_relayed, no_relay;

        public readonly RudpSocket socket;
        public IPEndPoint endPoint, localEnd, publicEnd;

        [Range(0, uint.MaxValue)] public uint u32_ip;
        [Range(0, ushort.MaxValue)] public ushort u16_port;

        public readonly ThreadSafe_struct<double>
            lastSend = new(),
            lastReceive = new();

        public readonly RudpChannel
            channel_files,
            channel_states,
            channel_flux,
            channel_audio;

        public readonly MemoryStream states_recStream;
        public readonly BinaryReader states_recReader;

        public override string ToString() => $"conn({socket.localPort}->{endPoint}{(is_relayed ? "[relayed]" : string.Empty)})";

        //----------------------------------------------------------------------------------------------------------

        internal RudpConnection(in RudpSocket socket, in IPEndPoint endPoint, in bool is_relayed, in bool no_relay)
        {
            this.socket = socket;
            this.endPoint = endPoint;
            this.is_relayed = is_relayed;
            this.no_relay = no_relay;

            u32_ip = (uint)endPoint.Address.Address;
            u16_port = (ushort)endPoint.Port;

#if UNITY_EDITOR
            _initiated = true;
            _is_relayed = is_relayed;
            _no_relay = no_relay;
#endif

            channel_files = new(this, RudpHeaderM.Files);
            channel_states = new(this, RudpHeaderM.States);
            channel_flux = new(this, RudpHeaderM.Flux);
            channel_audio = new(this, RudpHeaderM.Audio);

            states_recStream = new();
            states_recReader = new(states_recStream, Util_rudp.ENCODING, false);
        }

        //----------------------------------------------------------------------------------------------------------

        public void WriteConnection(in BinaryWriter writer)
        {
            writer.Write(is_relayed);
            writer.Write(publicEnd);
            writer.Write(localEnd);
        }

        public void AppendStatus(in StringBuilder log)
        {
            lock (this)
                channel_states.AppendStatesStatus(log);
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();

            channel_files.Dispose();
            channel_states.Dispose();
            channel_flux.Dispose();
            channel_audio.Dispose();

            states_recStream.Dispose();
            states_recReader.Dispose();
        }
    }
}