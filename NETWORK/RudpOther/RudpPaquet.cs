using System;
using System.IO;

namespace _RUDP_
{
    internal class RudpPaquet : IDisposable
    {
        public RudpHeader header;
        public readonly byte[] buffer = new byte[RudpSocket.BUFFER_SIZE];
        public readonly MemoryStream stream;
        public override string ToString() => $"{header} (size:{stream.Position})";
        public bool Pending => stream.Position > 0;

        public byte lastID;
        public double lastTime;
        public byte attempt;

        //----------------------------------------------------------------------------------------------------------

        public RudpPaquet()
        {
            stream = new(buffer);
        }

        //----------------------------------------------------------------------------------------------------------

        public void NewData(in RudpHeaderM mask, in BinaryReader reader)
        {
            lastTime = 0;
            attempt = 0;

            lastID = ++lastID == 0 ? (byte)1 : lastID;
            header = new(mask, lastID);

            stream.Position = RudpHeader.PREFIXE_LENGTH; 
            
            while (reader.BaseStream.Remaining() > 0)
            {
                ushort length = reader.ReadUInt16();
                if (length > stream.Length - stream.Position)
                {
                    reader.BaseStream.Position -= sizeof(ushort);
                    break;
                }
                else
                    reader.BaseStream.CopyTo(stream, length);
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}