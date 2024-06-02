using System.Collections.Generic;
using System.Net;
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

            eveComm.Push();

            if (connections.Count > 0)
            {
                HashSet<IPEndPoint> removeKeys = null;

                lock (connections)
                    foreach (var pair in connections)
                        lock (pair.Value.disposed)
                            if (pair.Value.disposed._value)
                                if (removeKeys == null)
                                    removeKeys = new() { pair.Key };
                                else
                                    removeKeys.Add(pair.Key);
                            else
                                pair.Value.Push();

                if (removeKeys != null)
                    foreach (IPEndPoint connector in removeKeys)
                        lock (connections)
                            connections.Remove(connector);
            }
        }
    }
}