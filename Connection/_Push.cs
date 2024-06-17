using _UTIL_;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpConnection
    {
        [Header("~@ Push @~")]
        public bool keepAlive;
        public readonly ThreadSafe<byte> keepalive_attempt = new();
        public bool IsAlive(in double milliseconds) => lastReceive.Value + milliseconds > Util.TotalMilliseconds;

        //----------------------------------------------------------------------------------------------------------

        public void Push()
        {
            channel_files.Push();
            channel_states.Push();

            if (keepAlive)
            {
                double time = Util.TotalMilliseconds;
                lock (lastSend)
                    lock (keepalive_attempt)
                    {
                        int freq = keepalive_attempt._value switch
                        {
                            0 => 50,
                            1 => 500,
                            2 => 1000,
                            3 => 2500,
                            _ => 5000,
                        };

                        if (time > lastSend._value + freq)
                        {
                            if (keepalive_attempt._value < 10)
                                ++keepalive_attempt._value;
                            Send(Util_rudp.EMPTY_BUFFER, 0, 0);
                        }
                    }
            }
        }

        public void Send(in byte[] buffer, in ushort offset, in ushort length)
        {
            lastSend.Value = Util.TotalMilliseconds;
            socket.SendTo(buffer, offset, length, endPoint);
        }
    }
}