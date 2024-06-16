using _UTIL_;
using System;
using System.Collections;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator EGetPublicEnd(Action<bool> onSuccess = null)
        {
            Debug.Log($"Querying public IP...".ToSubLog());
            conn.socket.selfConn.publicEnd = null;

            ThreadSafe<bool> done = new();

            lock (onAcks)
                onAcks[EveCodes.GetPublicEndPoint] = () =>
                {
                    done.Value = true;

                    IPEndPoint publicEnd = socketReader.ReadIPEndPoint();
                    conn.socket.selfConn.publicEnd = publicEnd;

                    if (!publicEnd.Address.Equals(Util_rudp.publicIP))
                        Debug.Log($"Public endpoint: {publicEnd}");
                    Util_rudp.publicIP = publicEnd.Address;
                };

            byte attempts = 0;
            while (!done.Value && ++attempts < 10)
            {
                lock (eveStream)
                {
                    NewPaquet();
                    eveWriter.Write((byte)EveCodes.GetPublicEndPoint);
                    SendPaquet();
                }
                float speed = attempts switch
                {
                    0 => 0,
                    1 => .1f,
                    2 => .2f,
                    3 => .35f,
                    4 => .5f,
                    _ => .8f,
                };
                WaitForSecondsRealtime wait = new(speed);
                while (wait.MoveNext())
                    yield return null;
            }

            if (done.Value)
                onSuccess?.Invoke(true);
            else
            {
                Debug.LogWarning("Failed to get public IP");
                onSuccess?.Invoke(false);
            }

            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    lock (onAcks)
                        onAcks.Remove(EveCodes.GetPublicEndPoint);
                },
            };
        }
    }
}