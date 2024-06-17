using System;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator EStartJoining(string hostName, int publicHash, int privateHash, Action<RudpConnection> onSuccess, Action onFailure)
        {
            bool failure = false;
            RudpConnection hostConn = null;

            while (true)
            {
                var eSend = ESendUntilAck(
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
                                    hostConn = conn.socket.ReadConnection(socketReader);
                                    hostConn.keepAlive = true;
                                    Debug.Log($"Start Holepunch-> {hostConn.endPoint}");
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
                                return;
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
                    yield return null;

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
                        yield return null;
                    else
                        break;
                }
            }
        }
    }
}