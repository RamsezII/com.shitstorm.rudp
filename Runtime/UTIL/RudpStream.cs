using _UTIL_;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

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

        readonly RudpConnection conn;
        readonly MemoryStream stream;
        readonly GZipStream gzip;
        readonly BinaryWriter writer_raw, writer_gzip;
        readonly BinaryReader reader_raw;
        public bool HasData => stream.Length > RudpHeader.HEADLEN_B;

        //----------------------------------------------------------------------------------------------------------

        public RudpStream()
        {
            stream = new();

            writer_raw = new(stream, Util_rudp.ENCODING, false);
            reader_raw = new(stream, Util_rudp.ENCODING, false);

            gzip = new(stream, CompressionMode.Compress, true);
            writer_gzip = new(gzip, Util_rudp.ENCODING, false);

            stream.SetLength(RudpHeader.HEADLEN_B);
            stream.Position = stream.Length;
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

        public PaquetBuffer ToReliablePaquet()
        {
            lock (this)
                return new(stream.GetBuffer(), 0, (ushort)Mathf.Min((int)stream.Length, Util_rudp.PAQUET_SIZE_BIG));
        }

        public void OnCleanAfterAck(in ushort paquet_size)
        {
            lock (this)
            {
                byte[] buffer = stream.GetBuffer();
                stream.Position = 0;
                Buffer.BlockCopy(buffer, paquet_size, buffer, RudpHeader.HEADLEN_B, (int)stream.Length - paquet_size);
                stream.SetLength(stream.Length - paquet_size + RudpHeader.HEADLEN_B);
                stream.Position = stream.Length;
            }
        }

        public void AppendStatus(in StringBuilder log)
        {
            lock (this)
                log.Append($"pending data: {(stream.Length - RudpHeader.HEADLEN_B).LogDataSize()}");
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