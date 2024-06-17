using _UTIL_;
using System;
using System.IO;
using System.IO.Compression;

namespace _RUDP_
{
    /// <summary>
    /// write compressible fragments of data into a stream, prefixed by their length.
    /// paquetLength is the length of the data being currently sent in loop until the corresponding ACK is received.
    /// </summary>
    public class RudpStream : Disposable
    {
        public enum Compressions { None, Gzip }
        const Compressions COMPRESSION = 0;

        readonly MemoryStream stream;
        readonly GZipStream gzip;
        readonly BinaryWriter writer_raw, writer_gzip;
        readonly BinaryReader reader_raw;
        public bool HasData => stream.Length > RudpHeader.HEADER_length;
        public string LogBytes() => GetPaquetBuffer()[RudpHeader.HEADER_length..].LogBytes();

        //----------------------------------------------------------------------------------------------------------

        public RudpStream()
        {
            stream = new();

            writer_raw = new(stream, Util_rudp.ENCODING, false);
            reader_raw = new(stream, Util_rudp.ENCODING, false);

            gzip = new(stream, CompressionMode.Compress, true);
            writer_gzip = new(gzip, Util_rudp.ENCODING, false);

            // header
            stream.WriteHeader();
        }

        //----------------------------------------------------------------------------------------------------------

        public void Write(in Action<BinaryWriter> onWriter)
        {
            lock (this)
            {
                ushort pos1 = (ushort)stream.Position;
                writer_raw.Write((ushort)0);

                onWriter(COMPRESSION switch
                {
                    Compressions.Gzip => writer_gzip,
                    _ => writer_raw
                });

                ushort pos2 = (ushort)stream.Position;
                ushort length = (ushort)(pos2 - pos1 - 2);

                if (length == 0)
                    stream.Position = pos1;
                else
                {
                    stream.Position = pos1;
                    writer_raw.Write(length);
                    stream.Position = pos2;
                }
            }
        }

        public byte[] GetPaquetBuffer()
        {
            lock (this)
                return stream.GetBuffer()[..(int)stream.Length];
        }

        public void OnCleanAfterAck(in ushort paquetSize)
        {
            lock (this)
            {
                byte[] buffer = stream.GetBuffer();
                stream.Position = 0;
                Buffer.BlockCopy(buffer, paquetSize, buffer, RudpHeader.HEADER_length, (int)stream.Length - paquetSize);
                stream.SetLength(stream.Length - paquetSize + RudpHeader.HEADER_length);
                stream.Position = stream.Length;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            stream.Dispose();
            writer_raw.Dispose();
            reader_raw.Dispose();
            gzip.Dispose();
            writer_gzip.Dispose();
        }
    }
}