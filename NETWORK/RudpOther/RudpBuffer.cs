using _UTIL_;
using System;
using System.IO;
using System.IO.Compression;

namespace _RUDP_
{
    public class RudpBuffer : Disposable
    {
        public readonly byte[] buffer;
        public readonly MemoryStream stream;
        public readonly BinaryWriter writer_raw;
        public readonly GZipStream gzip;
        public readonly BinaryWriter writer_gzip;
        public bool HasData => stream.Position > RudpHeader.HEADER_length;

        //----------------------------------------------------------------------------------------------------------

        public RudpBuffer()
        {
            buffer = new byte[RudpSocket.PAQUET_SIZE];
            stream = new(buffer);
            writer_raw = new(stream, RudpSocket.UTF8, false);
            gzip = new(stream, CompressionLevel.Fastest, true);
            writer_gzip = new(gzip, RudpSocket.UTF8, false);
            Flush();
        }

        //----------------------------------------------------------------------------------------------------------

        public void Flush() => stream.Position = RudpHeader.HEADER_length;

        public bool TryWrite(in Action<BinaryWriter> onWriter, in bool gzip = false)
        {
            ushort position = (ushort)stream.Position;
            try
            {
                onWriter(gzip ? writer_gzip : writer_raw);
                return true;
            }
            catch
            {
                stream.Position = position;
                return false;
            }
        }

        public byte[] GetPaquetBuffer()
        {
            lock (this)
                return buffer[..(int)stream.Position];
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            writer_gzip.Dispose();
            gzip.Dispose();
            writer_raw.Dispose();
            stream.Dispose();
        }
    }
}