using System;
using System.IO;

namespace _RUDP_
{
    public partial class RudpChannel : IDisposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly MemoryStream stream_data;
        public readonly BinaryReader reader_data;
        public readonly BinaryWriter writer_data;
        readonly RudpPaquet paquet;

        public byte last_recID = byte.MaxValue;

        public override string ToString() => $"{conn}[{mask}]";

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in RudpConnection conn, in RudpHeaderM mask)
        {
            this.mask = mask;
            this.conn = conn;
            stream_data = new();
            reader_data = new(stream_data, RudpSocket.UTF8, true);
            writer_data = new(stream_data, RudpSocket.UTF8, true);
            paquet = new(this);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            stream_data.Dispose();
            reader_data.Dispose();
            writer_data.Dispose();
            paquet.Dispose();
        }
    }
}