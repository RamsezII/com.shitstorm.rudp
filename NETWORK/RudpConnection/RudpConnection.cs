using _UTIL_;
using System.Net;

namespace _RUDP_
{
    public partial class RudpConnection : Disposable
    {
        public readonly RudpSocket socket;
        public readonly IPEndPoint endPoint;
        public IPEndPoint localEnd, publicEnd;

        public readonly ThreadSafe<double>
            lastSend = new(),
            lastReceive = new();

        public readonly RudpChannel
            channel_files,
            channel_states,
            channel_flux,
            channel_audio;

        public bool keepAlive;

        public bool IsAlive(in double delay) => lastReceive.Value + delay < Util.TotalMilliseconds;

        public override string ToString() => $"conn({socket.endIP_LAN}->{endPoint})";

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint endPoint)
        {
            this.socket = socket;
            this.endPoint = endPoint;

            channel_files = new(this, RudpHeaderM.Files);
            channel_states = new(this, RudpHeaderM.States);
            channel_flux = new(this, RudpHeaderM.Flux);
            channel_audio = new(this, RudpHeaderM.Audio);
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            channel_files.Dispose();
            channel_states.Dispose();
            channel_flux.Dispose();
            channel_audio.Dispose();
        }
    }
}