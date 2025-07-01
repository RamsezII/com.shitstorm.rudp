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

            lock (conns_set)
                if (conns_set.Count > 0)
                    foreach (RudpConnection conn in conns_set)
                        lock (conn)
                            if (!conn._disposed)
                                conn.Push();
        }
    }
}