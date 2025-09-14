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

        public RudpConnection ToConnection(in IPEndPoint remoteEnd, in bool no_relay, out bool is_new)
        {
            lock (conns_dic)
            {
                if (conns_dic.TryGetValue(remoteEnd, out RudpConnection conn))
                    is_new = false;
                else
                {
                    conn = new RudpConnection(this, remoteEnd, no_relay);
                    conns_dic[remoteEnd] = conn;

                    lock (NUCLEOR.instance.mainThreadLock)
                        NUCLEOR.delegates.LateUpdate_onEndOfFrame_once += () =>
                        {
                            lock (conns_set)
                                conns_set.Add(conn);
                        };

                    if (!use_relay)
                    {
                        if (remoteEnd.Address.Equals(IPAddress.Loopback))
                            conns_dic[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                        if (remoteEnd.Address.Equals(IPAddress.Any))
                            conns_dic[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;
                    }

                    is_new = true;
                    Debug.Log($"new {conn}");
                }

                return conn;
            }
        }

        public RudpConnection ReadConnection(in BinaryReader reader, out bool is_new)
        {
            IPEndPoint
                publicEnd = reader.ReadIPEndPoint(),
                localEnd = reader.ReadIPEndPoint(),
                endPoint;

            if (use_relay)
                endPoint = publicEnd;
            else if (!publicEnd.Address.Equals(Util_rudp.publicIP) || !Util_rudp.IsSameSubnet24(Util_rudp.localIP, localEnd.Address, true))
                endPoint = publicEnd;
            else if (localEnd.Address.Equals(Util_rudp.localIP))
                endPoint = new(IPAddress.Loopback, localEnd.Port);
            else
                endPoint = localEnd;

            RudpConnection conn = ToConnection(endPoint, false, out is_new);
            conn.localEnd = localEnd;
            conn.publicEnd = publicEnd;
            return conn;
        }
    }
}