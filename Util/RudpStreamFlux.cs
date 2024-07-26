using _UTIL_;
using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public class RudpStreamFlux : Disposable
    {
        readonly MemoryStream stream;
        readonly BinaryWriter writer;
        readonly BinaryReader reader;

        //----------------------------------------------------------------------------------------------------------

        public RudpStreamFlux()
        {
            stream = new();
            writer = new(stream, Util_rudp.ENCODING, false);
            reader = new(stream, Util_rudp.ENCODING, false);
            writer.Write((uint)0);
        }

        //----------------------------------------------------------------------------------------------------------

        public void WriteData(in Action<BinaryWriter> onWriter)
        {
            lock (stream)
            {
                ushort pos1 = (ushort)stream.Position;
                writer.Write((ushort)0);
                onWriter(writer);
                ushort pos2 = (ushort)stream.Position;
                ushort length = (ushort)(pos2 - pos1 - 2);

                if (length > Util_rudp.DATA_SIZE_BIG)
                {
                    Debug.LogError($"Flux fragment too long: {length} > {Util_rudp.DATA_SIZE_BIG}");
                    stream.Position = pos1;
                    stream.SetLength(pos1);
                }
                else
                {
                    stream.Position = pos1;
                    writer.Write(length);
                    stream.Position = pos2;
                }
            }
        }

        public bool TryPullPaquet(out byte[] paquet)
        {
            lock (stream)
                if (stream.Length > 2)
                {
                    stream.Position = RudpHeader.HEADER_length;

                    while (stream.Position < Util_rudp.PAQUET_SIZE_BIG && stream.Length - stream.Position > 2)
                    {
                        ushort length = reader.ReadUInt16();
                        if (stream.Position + length <= Util_rudp.PAQUET_SIZE_BIG)
                            stream.Position += length;
                        else
                        {
                            stream.Position -= 2;
                            break;
                        }
                    }

                    if (stream.Position > RudpHeader.HEADER_length)
                    {
                        paquet = stream.GetBuffer()[..(int)stream.Position];
                        return true;
                    }
                }
            paquet = null;
            return false;
        }

        public void CleanOldData()
        {
            lock (stream)
            {
                stream.Position = 0;
                byte[] buffer = stream.GetBuffer();
                ushort offset = (ushort)stream.Position;
                Buffer.BlockCopy(buffer, offset, buffer, RudpHeader.HEADER_length, (int)(stream.Length - offset));
                stream.SetLength(stream.Length - offset + RudpHeader.HEADER_length);
                stream.Position = stream.Length;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            stream.Dispose();
            writer.Dispose();
            reader.Dispose();
        }
    }
}