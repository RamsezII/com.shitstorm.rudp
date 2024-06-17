using _UTIL_;
using System;
using System.IO;
using System.Net;

namespace _RUDP_
{
    [Serializable]
    public partial class RudpConnection : Disposable
    {
        public readonly RudpSocket socket;
        public IPEndPoint endPoint, localEnd, publicEnd;

        public ThreadSafe<double>
            lastSend = new(),
            lastReceive = new();

        public readonly RudpChannel
            channel_files,
            channel_states,
            channel_flux,
            channel_audio;

        public readonly MemoryStream states_recStream;
        public readonly BinaryReader states_recReader;

        public override string ToString() => $"conn({socket.localPort}->{endPoint})";

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint endPoint)
        {
            this.socket = socket;
            this.endPoint = endPoint;

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
            writer.Write(publicEnd);
            writer.Write(localEnd);
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