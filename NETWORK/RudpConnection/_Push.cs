namespace _RUDP_
{
    partial class RudpConnection
    {
        public void Push()
        {
            channel_files.Push();
            channel_states.Push();

            if (keepAlive)
            {
                double time = Util.TotalMilliseconds;
                lock (lastSend)
                    if (time > lastSend._value + 5000)
                    {
                        lastSend._value = time;
                        socket.SendTo(Util_rudp.EMPTY_BUFFER, 0, 0, endPoint);
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