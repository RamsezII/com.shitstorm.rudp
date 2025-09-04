using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator<float> EJoinPublicHost(string hostName, int publicHash, int privateHash, Action<RudpConnection> onSuccess, Action onFailure)
        {
            bool failure = false;
            RudpConnection hostConn = null;

            while (true)
            {
                float max_delay = 3;

                IEnumerator<float> eSend = ESendUntilAck(
                    writer =>
                    {
                        eveWriter.Write((byte)EveCodes.JoinHost);
                        eveWriter.WriteText(hostName);
                        eveWriter.Write(publicHash);
                        eveWriter.WriteIPEnd(conn.socket.selfConn.localEnd);
                    },
                    reader =>
                    {
                        ReadPublicEnd();
                        AckCodes ack = (AckCodes)socketReader.ReadByte();

                        switch (ack)
                        {
                            case AckCodes.Confirm:
                                {
                                    hostConn = conn.socket.ReadConnection(socketReader, out _);
                                    hostConn.keepAlive = true;
                                    Debug.Log($"[EVE_CONFIRM] Start Holepunch-> {hostConn.endPoint}");
                                }
                                break;

                            case AckCodes.Reject:
                                Debug.LogWarning("Joining host rejected");
                                break;

                            case AckCodes.HostNotFound:
                                Debug.LogWarning("Host not found");
                                break;

                            case AckCodes.HostPassMismatch:
                                Debug.LogWarning("Host password mismatch");
                                break;

                            default:
                                Debug.LogWarning($"Unexpected ack: \"{ack}\"");
                                break;
                        }

                        if (ack != AckCodes.Confirm)
                        {
                            onFailure?.Invoke();
                            failure = true;
                        }
                    },
                    () =>
                    {
                        Debug.LogWarning("Failed to start joining");
                        onFailure?.Invoke();
                        failure = true;
                    });

                while (eSend.MoveNext())
                {
                    yield return eSend.Current;

                    max_delay -= Time.unscaledDeltaTime;
                    if (max_delay < 0)
                    {
                        onFailure?.Invoke();
                        failure = true;
                        yield break;
                    }
                }

                lock (mainLock)
                    if (failure)
                        yield break;

                var wait = new WaitForSecondsRealtime(1);
                while (true)
                {
                    if (hostConn.IsAlive(1000))
                    {
                        onSuccess?.Invoke(hostConn);
                        yield break;
                    }

                    if (wait.MoveNext())
                        yield return 0;
                    else
                        break;
                }
            }
        }
    }
}