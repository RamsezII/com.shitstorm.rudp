using System.IO;

namespace _RUDP_
{
    partial class RudpSocket
    {
        public bool TryReadData(out BinaryReader reader, out ushort length)
        {
            lock (recDataStream)
                if (recDataStream.Position < 2)
                {
                    reader = null;
                    length = 0;
                    return false;
                }
                else
                {
                    length = recDataReader.ReadUInt16();
                    if (recDataStream.Length < recDataStream.Position + length)
                    {
                        recDataStream.Position -= 2;
                        reader = null;
                        return false;
                    }
                    reader = recDataReader;
                    return true;
                }
        }
    }
}