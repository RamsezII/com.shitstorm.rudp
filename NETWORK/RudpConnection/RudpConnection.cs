using _UTIL_;
using System;
using System.Net;

namespace _RUDP_
{
    public partial class RudpConnection : IDisposable
    {
        public readonly RudpSocket socket;
        public readonly IPEndPoint endPoint;
        public IPEndPoint localEnd, publicEnd;

        public readonly ThreadSafe<bool> disposed = new();

        public readonly ThreadSafe<double>
            lastSend = new(),
            lastReceive = new();

        public override string ToString() => $"conn({socket.endIP_LAN}->{endPoint})";

        public bool keepAlive;

        public RudpChannel
            channel_direct,
            channel_mainthread;

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint endPoint)
        {
            this.socket = socket;
            this.endPoint = endPoint;

            channel_direct = new(this, RudpHeaderM.Files);
            channel_mainthread = new(this, RudpHeaderM.States);

            keepAlive = true;
        }

        //----------------------------------------------------------------------------------------------------------

        public void OnNetworkPush()
        {
            channel_direct.TryPushDataIntoPaquet();
            channel_mainthread.TryPushDataIntoPaquet();

            if (keepAlive)
            {
                double time = Util_net.TotalMilliseconds;
                if (time > lastSend.Value + 5000)
                {
                    lastSend.Value = time;
                    socket.Send(Util_net.EMPTY_BUFFER);
                }
            }
        }

        public void Send(in byte[] buffer, in ushort offset, in ushort length)
        {
            lastSend.Value = Util_net.TotalMilliseconds;
            socket.SendTo(buffer, offset, length, endPoint);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;

            channel_direct.Dispose();
            channel_mainthread.Dispose();
        }
    }
}