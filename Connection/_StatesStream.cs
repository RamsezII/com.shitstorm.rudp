using System;
using System.IO;

namespace _RUDP_
{
    partial class RudpConnection
    {
        public bool HasStates()
        {
            lock (states_recStream)
                return states_recStream.Length > 2;
        }

        public bool TryPullStates(in Action<BinaryReader, long> onReader)
        {
            lock (states_recStream)
                if (HasStates())
                {
                    states_recStream.Position = 0;
                    ushort length = states_recReader.ReadUInt16();

                    if (states_recStream.Length < states_recStream.Position + length)
                    {
                        states_recStream.Position = states_recStream.Length;
                        return false;
                    }

                    length += (ushort)states_recStream.Position;
                    onReader(states_recReader, length);

                    states_recStream.Position = 0;
                    byte[] buffer = states_recStream.GetBuffer();
                    Buffer.BlockCopy(buffer, length, buffer, 0, (int)(states_recStream.Length - length));
                    states_recStream.SetLength(states_recStream.Length - length);
                    states_recStream.Position = states_recStream.Length;

                    return true;
                }
            return false;
        }
    }
}