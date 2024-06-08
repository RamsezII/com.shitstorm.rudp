using System.IO;

namespace _RUDP_
{
    partial class RudpSocket
    {
        public bool TryReadData(out BinaryReader reader, out ushort length)
        {
            lock (states_recStream)
                if (states_recStream.Position < 2)
                {
                    reader = null;
                    length = 0;
                    return false;
                }
                else
                {
                    length = states_recReader.ReadUInt16();
                    if (states_recStream.Length < states_recStream.Position + length)
                    {
                        states_recStream.Position -= 2;
                        reader = null;
                        return false;
                    }
                    reader = states_recReader;
                    return true;
                }
        }
    }
}