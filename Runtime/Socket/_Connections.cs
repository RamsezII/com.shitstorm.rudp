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

        public RudpConnection ToConnection(in IPEndPoint remoteEnd, in bool use_relay, out bool is_new)
        {
            lock (conns_dic)
            {
                if (conns_dic.TryGetValue(remoteEnd, out RudpConnection conn))
                {
                    is_new = false;
                    if (use_relay != conn.is_relayed)
                        Debug.LogError($"relay conflict: {nameof(use_relay)}={use_relay} ; {nameof(conn)}.{nameof(conn.is_relayed)}={conn.is_relayed}");
                }
                else
                {
                    conn = new RudpConnection(this, remoteEnd, use_relay);
                    conns_dic[remoteEnd] = conn;

                    lock (NUCLEOR.instance.mainThreadLock)
                        NUCLEOR.delegates.LateUpdate_onEndOfFrame_once += () =>
                        {
                            lock (conns_set)
                                conns_set.Add(conn);
                        };

                    if (remoteEnd.Address.Equals(IPAddress.Loopback))
                        conns_dic[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                    if (remoteEnd.Address.Equals(IPAddress.Any))
                        conns_dic[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;

                    is_new = true;
                    Debug.Log($"new {conn}");
                }

                return conn;
            }
        }

        public RudpConnection ReadConnection(in BinaryReader reader, out bool is_new)
        {
            bool is_relayed = reader.ReadBoolean();

            IPEndPoint
                publicEnd = reader.ReadIPEndPoint(),
                localEnd = reader.ReadIPEndPoint(),
                endPoint;

            if (publicEnd.Address.Equals(Util_rudp.publicIP) && Util_rudp.IsSameSubnet24(Util_rudp.localIP, localEnd.Address, true))
                if (localEnd.Address.Equals(Util_rudp.localIP))
                    endPoint = new(IPAddress.Loopback, localEnd.Port);
                else
                    endPoint = localEnd;
            else
                endPoint = publicEnd;

            RudpConnection conn = ToConnection(endPoint, is_relayed, out is_new);
            conn.localEnd = localEnd;
            conn.publicEnd = publicEnd;
            return conn;
        }
    }
}