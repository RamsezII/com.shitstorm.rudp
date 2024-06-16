namespace _RUDP_
{
    partial class EveComm
    {
        void NewPaquet()
        {
            sendFlag = true;
            eveStream.Position = HEADER_LENGTH;
            eveBuffer[1] = ++id == 0 ? (byte)1 : id;
        }

        public void Push()
        {
            lock (pushLock)
                lock (eveStream)
                    if (eveStream.Position > HEADER_LENGTH)
                        lock (this)
                        {
                            double time = Util.TotalMilliseconds;
                            lock (lastSend)
                                if (sendFlag || time > lastSend._value + 1000)
                                    SendPaquet();
                        }
        }

        void SendPaquet()
        {
            sendFlag = false;
            lastSend._value = Util.TotalMilliseconds;
            conn.Send(eveBuffer, 0, (ushort)eveStream.Position);
        }
    }
}