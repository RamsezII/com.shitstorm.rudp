using System.IO;
using System;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        public void EnqueueData(in Action<BinaryWriter> onWriter, bool directPushAttempt = true)
        {
            lock (stream)
            {
                writer.BeginWrite(out ushort prefixePos);
                onWriter(writer);
                writer.EndWriteRUDP(prefixePos);
            }
            if (directPushAttempt)
                TryPushDataIntoPaquet();
        }
    }
}