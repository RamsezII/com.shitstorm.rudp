using _RUDP_;
using System.Collections.Generic;
using System.IO;

namespace _RUDP_
{
    public interface INetworkable
    {
        void OnWriter(in BinaryWriter writer);
        void OnReader(in BinaryReader reader, in RudpConnection recConn);
    }
}

public static partial class Util_rudp
{
    public static readonly Dictionary<int, INetworkable> netSingletons = new();
}