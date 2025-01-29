using _ARK_;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public readonly Dictionary<IPEndPoint, RudpConnection> conns_dic = new();
        public readonly HashSet<RudpConnection> conns_set = new();

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection ToConnection(in IPEndPoint remoteEnd, out bool isNew)
        {
            lock (conns_dic)
            {
                if (conns_dic.TryGetValue(remoteEnd, out RudpConnection conn))
                    isNew = false;
                else
                {
                    conn = new RudpConnection(this, remoteEnd);
                    conns_dic[remoteEnd] = conn;

                    lock (NUCLEOR.instance.mainThreadLock)
                        NUCLEOR.onEndOfFrame += () =>
                        {
                            lock (conns_set)
                                conns_set.Add(conn);
                        };

                    if (remoteEnd.Address.Equals(IPAddress.Loopback))
                        conns_dic[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                    if (remoteEnd.Address.Equals(IPAddress.Any))
                        conns_dic[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;

                    isNew = true;
                    Debug.Log($"new conn {conn}");
                }
                return conn;
            }
        }

        public RudpConnection ReadConnection(in BinaryReader reader, out bool isNew)
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

            RudpConnection conn = ToConnection(endPoint, out isNew);
            conn.localEnd = localEnd;
            conn.publicEnd = publicEnd;
            return conn;
        }
    }
}