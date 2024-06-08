using System.Collections.Generic;
using System.IO;
using System.Net;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public readonly Dictionary<IPEndPoint, RudpConnection> connections = new();

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection ToConnection(in IPEndPoint remoteEnd) => ToConnection(remoteEnd, out _);
        public RudpConnection ToConnection(in IPEndPoint remoteEnd, out bool isnew)
        {
            lock (connections)
            {
                if (connections.TryGetValue(remoteEnd, out RudpConnection conn))
                    isnew = false;
                else
                {
                    conn = new RudpConnection(this, remoteEnd);
                    connections[remoteEnd] = conn;

                    if (remoteEnd.Address.Equals(IPAddress.Loopback))
                        connections[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                    if (remoteEnd.Address.Equals(IPAddress.Any))
                        connections[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;

                    isnew = true;
                }
                return conn;
            }
        }

        public RudpConnection ReadConnection(in BinaryReader reader)
        {
            IPEndPoint
                publicEnd = reader.ReadIPEndPoint(),
                localEnd = reader.ReadIPEndPoint(),
                endPoint;

            if (publicEnd.Address.Equals(Util_rudp.publicIP))
                if (localEnd.Address.Equals(Util_rudp.localIP))
                    endPoint = new(IPAddress.Loopback, localEnd.Port);
                else
                    endPoint = localEnd;
            else
                endPoint = publicEnd;

            return ToConnection(endPoint);
        }
    }
}