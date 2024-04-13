using System;
using System.IO;

namespace _RUDP_
{
    public partial class RudpChannel : IDisposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly MemoryStream stream;
        readonly BinaryReader reader;
        public readonly BinaryWriter writer;

        public byte last_recID = byte.MaxValue;

        public override string ToString() => $"{conn}[{mask}]";

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in RudpConnection conn, in RudpHeaderM mask)
        {
            this.mask = mask;
            this.conn = conn;
            stream = new();
            reader = new(stream, RudpSocket.UTF8, true);
            writer = new(stream, RudpSocket.UTF8, true);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            stream.Dispose();
            reader.Dispose();
            writer.Dispose();
            paquet.Dispose();
        }
    }
}