using _UTIL_;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpConnection
    {
        [Header("~@ Push @~")]
        public bool keepAlive;
        public readonly ThreadSafe_struct<byte> keepalive_attempt = new();
        public bool IsAlive(in double milliseconds) => Util.TotalMilliseconds < lastReceive.Value + milliseconds;

        //----------------------------------------------------------------------------------------------------------

        public void Push()
        {
            channel_states.PushStates();

            if (keepAlive)
                lock (keepalive_attempt)
                {
                    int freq = keepalive_attempt._value switch
                    {
                        0 => 1500,
                        1 => 2000,
                        2 => 3500,
                        _ => 4500,
                    };

                    double time = Util.TotalMilliseconds;
                    if (time > lastSend.Value + freq)
                    {
                        if (RudpSocket.h_settings.logKeepAlives)
                            Debug.Log($"{this} keepalive attempt {keepalive_attempt.Value}".ToSubLog());

                        if (keepalive_attempt.Value < 10)
                            ++keepalive_attempt.Value;

                        if (this == socket.eveComm.conn || this == socket.relayConn)
                            Send(Util_rudp.EMPTY_ZERO, 0, 0);
                        else
                            Send(Util_rudp.EMPTY_LONG, 0, RudpHeader.HEADLEN_B);
                    }
                }
        }

        public void Send(in byte[] buffer, in ushort offset, in ushort length)
        {
            lastSend.Value = Util.TotalMilliseconds;
            socket.SendTo(buffer, offset, length, no_relay, endPoint);
        }

        public void Send_direct(in byte[] buffer, in ushort offset, in ushort length, in IPEndPoint endPoint)
        {
            lastSend.Value = Util.TotalMilliseconds;
            socket.SendTo(buffer, offset, length, SocketFlags.None, endPoint);
        }
    }
}