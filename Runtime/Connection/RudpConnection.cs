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
#endif
        public readonly bool is_relayed;
        public byte header_length = RudpHeader.HEADER_length;

        public readonly RudpSocket socket;
        public IPEndPoint endPoint, localEnd, publicEnd;

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

        internal RudpConnection(in RudpSocket socket, in IPEndPoint endPoint, in bool is_relayed)
        {
            this.socket = socket;
            this.endPoint = endPoint;
            this.is_relayed = is_relayed;

#if UNITY_EDITOR
            _initiated = true;
            _is_relayed = is_relayed;
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