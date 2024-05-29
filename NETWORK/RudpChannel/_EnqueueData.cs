using System.IO;
using System;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        /// <summary>
        /// each data is prefixed by an ushort indicating the size of the data
        /// </summary>
        public void EnqueueData(in Action<BinaryWriter> onWriter, bool directPushAttempt = true)
        {
            lock (stream_data)
            {
                ushort prefixePos = (ushort)stream_data.Position;
                writer_data.Write((ushort)0);
                onWriter(writer_data);
                ushort suffixePos = (ushort)stream_data.Position;
                stream_data.Position = prefixePos;
                writer_data.Write((ushort)(suffixePos - prefixePos - sizeof(ushort)));
                stream_data.Position = suffixePos;
            }
            if (directPushAttempt)
                TryPushDataIntoPaquet();
        }
    }
}