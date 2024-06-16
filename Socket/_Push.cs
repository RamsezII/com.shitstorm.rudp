using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public void Push()
        {
            if (disposed.Value)
            {
                Debug.LogWarning($"{this} {nameof(Push)}: Disposed socket");
                return;
            }

            if (conns_set.Count > 0)
                foreach (RudpConnection conn in conns_set)
                    lock (conn.disposed)
                        if (conn.disposed._value)
                            lock (conns_dic)
                                conns_dic.Remove(conn.endPoint);
                        else
                            conn.Push();
        }
    }
}