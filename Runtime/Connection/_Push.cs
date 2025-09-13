using _UTIL_;
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
                        if (socket.settings.logKeepAlives)
                            Debug.Log($"{this} keepalive attempt {keepalive_attempt._value}".ToSubLog());
                        if (keepalive_attempt._value < 10)
                            ++keepalive_attempt._value;
                        Send(Util_rudp.EMPTY_BUFFER, 0, RudpHeader.HEADLEN_B, false);
                    }
                }
        }

        public void Send(in byte[] buffer, in ushort offset, in ushort length, in bool force_no_relay)
        {
            lastSend.Value = Util.TotalMilliseconds;
            socket.SendTo(buffer, offset, length, is_relayed, force_no_relay, endPoint);
        }
    }
}