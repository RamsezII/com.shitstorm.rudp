using _UTIL_;

namespace _RUDP_
{
    partial class RudpConnection
    {
        public readonly ThreadSafe<byte> keepalive_attempt = new();

        //----------------------------------------------------------------------------------------------------------

        public void Push()
        {
            channel_files.Push();
            channel_states.Push();

            if (keepAlive)
            {
                double time = Util.TotalMilliseconds;
                lock (lastSend)
                {
                    int freq = keepalive_attempt.Value switch
                    {
                        0 => 50,
                        1 => 500,
                        2 => 1000,
                        3 => 2500,
                        _ => 5000,
                    };

                    if (time > lastSend._value + freq)
                    {
                        ++keepalive_attempt.Value;
                        lastSend._value = time;
                        socket.SendTo(Util_rudp.EMPTY_BUFFER, 0, 0, endPoint);
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