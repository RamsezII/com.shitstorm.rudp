using System;
using System.IO;

namespace _RUDP_
{
    /// <summary>
    /// buffer for the data to be sent in loop until acknowledgement is received
    /// </summary>
    internal class RudpPaquet : IDisposable
    {
        public readonly RudpChannel channel;
        public RudpHeader header;
        public readonly byte[] buffer = new byte[RudpSocket.PAQUET_SIZE];
        public readonly MemoryStream stream;
        public override string ToString() => $"{header} (size:{stream.Position})";
        public bool Pending => stream.Position > 0;

        public byte lastID;
        public double lastTime;
        public byte attempt;

        //----------------------------------------------------------------------------------------------------------

        public RudpPaquet(in RudpChannel channel)
        {
            this.channel = channel;
            stream = new(buffer);
        }

        //----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// pull the data to be sent
        /// </summary>
        public void PullData()
        {
            lastTime = 0;
            attempt = 0;

            lastID = ++lastID == 0 ? (byte)1 : lastID;
            header = new(channel.mask, lastID);

            stream.Position = RudpHeader.HEADER_length;

            while (stream.Remaining() > 0)
            {
                ushort length = channel.reader_data.ReadUInt16();
                if (stream.Length + length > RudpSocket.PAQUET_SIZE)
                {
                    channel.reader_data.BaseStream.Position -= sizeof(ushort);
                    break;
                }
                else
                    channel.reader_data.BaseStream.CopyTo(stream, length);
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}