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

        public readonly MemoryStream stream;
        readonly GZipStream gzip;
        readonly BinaryWriter writer_raw, writer_gzip;
        readonly BinaryReader reader_raw;
        public bool HasData => stream.Position > RudpHeader.HEADER_length;

        //----------------------------------------------------------------------------------------------------------

        public RudpStream()
        {
            stream = new();

            writer_raw = new(stream, RudpSocket.UTF8, false);
            reader_raw = new(stream, RudpSocket.UTF8, false);

            gzip = new(stream, CompressionMode.Compress, true);
            writer_gzip = new(gzip, RudpSocket.UTF8, false);

            // header
            writer_raw.Write((uint)0);
        }

        //----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// add data, prefixed by its length, to the stream.
        /// </summary>
        /// <param name="gzip">gzip the data</param>
        public void Write(in Action<BinaryWriter> onWriter, in Compressions compression = 0)
        {
            lock (this)
            {
                ushort pos1 = (ushort)stream.Position;
                writer_raw.Write((ushort)0);
                writer_raw.Write((byte)compression);

                onWriter(compression switch
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
                return stream.GetBuffer()[..(int)stream.Position];
        }

        public void OnCleanAfterAck(in ushort paquetLength)
        {
            lock (this)
            {
                byte[] buffer = stream.GetBuffer();
                stream.Position = 0;
                Buffer.BlockCopy(buffer, paquetLength + RudpHeader.HEADER_length, buffer, RudpHeader.HEADER_length, (int)stream.Length - paquetLength - RudpHeader.HEADER_length);
                stream.SetLength(stream.Length - paquetLength);
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