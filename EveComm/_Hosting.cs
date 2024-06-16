using _UTIL_;
using System;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        bool hosting;
        string hostName;
        int publicHash, privateHash;

        //--------------------------------------------------------------------------------------------------------------

        public IEnumerator EStartHosting(string hostName, int publicHash, int privateHash, Action<bool> onSuccess)
        {
            this.hostName = hostName;
            this.publicHash = publicHash;
            this.privateHash = privateHash;

            return ESendUntilAck(
                EveCodes.AddHost,
                writer =>
                {
                    eveWriter.Write((byte)EveCodes.AddHost);
                    eveWriter.WriteIPEnd(conn.socket.selfConn.localEnd);
                    eveWriter.WriteText(hostName);
                    eveWriter.Write(publicHash);
                },
                reader =>
                {
                    ReadPublicEnd();
                    AckCodes ack = (AckCodes)socketReader.ReadByte();

                    switch (ack)
                    {
                        case AckCodes.Confirm:
                            Debug.Log("Host confirmed");
                            break;
                        case AckCodes.Reject:
                            Debug.LogWarning("Host rejected");
                            break;
                        case AckCodes.HostAlreadyExists:
                            Debug.LogWarning("Host already exists");
                            break;
                        default:
                            Debug.LogWarning($"Unexpected ack: \"{ack}\"");
                            return;
                    }

                    onSuccess?.Invoke(ack == AckCodes.Confirm);
                },
                () =>
                {
                    Debug.LogWarning("Failed to start hosting");
                    onSuccess?.Invoke(false);
                });
        }

        void ReceiveHolepunch()
        {
            if (hosting)
            {
                RudpConnection hostConn = conn.socket.ReadConnection(socketReader);
                hostConn.keepAlive = true;
                Debug.Log($"Joining host confirmed ({hostConn})");
            }
            else
                Debug.LogWarning("Received holepunch without hosting");
        }
    }
}