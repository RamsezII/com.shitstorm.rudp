using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public void Update()
        {
            if (disposed.Value)
            {
                Debug.LogWarning($"{this} {nameof(Update)}: Disposed socket");
                return;
            }

            eveClient.Update();

            if (connections.Count > 0)
            {
                HashSet<IPEndPoint> removeKeys = null;

                lock (connections)
                    foreach (var pair in connections)
                        lock (pair.Value.disposed)
                            if (pair.Value.disposed._value)
                                if (removeKeys == null)
                                    removeKeys = new HashSet<IPEndPoint> { pair.Key };
                                else
                                    removeKeys.Add(pair.Key);
                            else
                                pair.Value.OnNetworkPush();

                if (removeKeys != null)
                    foreach (IPEndPoint connector in removeKeys)
                        lock (connections)
                            connections.Remove(connector);
            }
        }
    }
}