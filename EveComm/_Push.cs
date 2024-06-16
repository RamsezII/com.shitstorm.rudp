namespace _RUDP_
{
    partial class EveComm
    {
        void NewHeader()
        {
            sendFlag = true;
            eveStream.Position = HEADER_LENGTH;
            eveBuffer[1] = ++id == 0 ? (byte)1 : id;
        }

        public void Push()
        {
            lock (eveStream)
                if (eveStream.Position > HEADER_LENGTH)
                    lock (this)
                    {
                        double time = Util.TotalMilliseconds;
                        lock (lastSend)
                            if (sendFlag || time > lastSend._value + 1000)
                            {
                                sendFlag = false;
                                lastSend._value = time;
                                conn.Send(eveBuffer, 0, (ushort)eveStream.Position);
                            }
                    }
        }
    }
}