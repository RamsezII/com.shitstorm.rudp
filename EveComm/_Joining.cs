using System;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator EStartJoining(string hostName, int publicHash, int privateHash, Action<bool> onSuccess) => ESendUntilAck(
            writer =>
            {
                eveWriter.Write((byte)EveCodes.JoinHost);
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
                        hosting.Value = false;
                        Debug.Log("Joining host confirmed");
                        break;
                    case AckCodes.Reject:
                        Debug.LogWarning("Joining host rejected");
                        break;
                    case AckCodes.HostNotFound:
                        Debug.LogWarning("Host not found");
                        break;
                    default:
                        Debug.LogWarning($"Unexpected ack: \"{ack}\"");
                        return;
                }
                onSuccess?.Invoke(ack == AckCodes.Confirm);
            },
            () =>
            {
                Debug.LogWarning("Failed to start joining");
                onSuccess?.Invoke(false);
            });

        void ReceiveHolepunch()
        {
            if (hosting.Value)
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