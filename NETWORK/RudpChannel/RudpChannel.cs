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

        public readonly byte[] buffer_paquet;
        public readonly MemoryStream stream_paquet;
        public bool Pending => stream_paquet.Position > 0;

        public double lastSend;
        public byte id, attempt;
        public override string ToString() => $"{conn}[{mask}]";

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in RudpConnection conn, in RudpHeaderM mask)
        {
            this.conn = conn;
            this.mask = mask;

            stream_data = new();
            reader_data = new(stream_data, RudpSocket.UTF8, true);
            writer_data = new(stream_data, RudpSocket.UTF8, true);

            buffer_paquet = new byte[RudpSocket.PAQUET_SIZE];
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            stream_data.Dispose();
            reader_data.Dispose();
            writer_data.Dispose();
            stream_paquet.Dispose();
        }
    }
}