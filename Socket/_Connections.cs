using System.Collections.Generic;
using System.IO;
using System.Net;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        readonly Dictionary<IPEndPoint, RudpConnection> conns_dic = new();
        readonly HashSet<RudpConnection> conns_set = new();
        public readonly IEnumerable<RudpConnection> ebroadcast;

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection ToConnection(in IPEndPoint remoteEnd) => ToConnection(remoteEnd, out _);
        public RudpConnection ToConnection(in IPEndPoint remoteEnd, out bool isnew)
        {
            lock (conns_dic)
            {
                if (conns_dic.TryGetValue(remoteEnd, out RudpConnection conn))
                    isnew = false;
                else
                {
                    conn = new RudpConnection(this, remoteEnd);
                    conns_dic[remoteEnd] = conn;
                    lock (conns_set)
                        conns_set.Add(conn);

                    if (remoteEnd.Address.Equals(IPAddress.Loopback))
                        conns_dic[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                    if (remoteEnd.Address.Equals(IPAddress.Any))
                        conns_dic[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;

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