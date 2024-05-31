using System;
using System.IO;

namespace _RUDP_
{
    public partial class EveClient
    {
        readonly byte[] eveBuffer;
        readonly MemoryStream eveStream;
        readonly BinaryWriter eveWriter;

        //----------------------------------------------------------------------------------------------------------

        public bool TryWrite(in Action<BinaryWriter> onWriter)
        {
            lock (eveBuffer)
            {
                ushort position = (ushort)eveStream.Position;
                try
                {
                    onWriter(eveWriter);
                    pending = true;
                    return true;
                }
                catch
                {
                    eveStream.Position = position;
                    return false;
                }
            }
        }
    }
}