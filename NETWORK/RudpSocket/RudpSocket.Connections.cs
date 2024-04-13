using System.Collections.Generic;
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
    }
}