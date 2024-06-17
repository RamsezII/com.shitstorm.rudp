using _UTIL_;
using System;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        readonly ThreadSafe<bool> hosting = new();

        //--------------------------------------------------------------------------------------------------------------

        public IEnumerator EStartHosting(string hostName, int publicHash, int privateHash, Action<bool> onSuccess) => ESendUntilAck(
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

        public IEnumerator EMaintainHosting()
        {
            Debug.Log("Maintaining host...".ToSubLog());
            hosting.Value = true;
            while (hosting.Value)
            {
                var wait = new WaitForSecondsRealtime(4);
                while (wait.MoveNext())
                    yield return null;

                if (!hosting.Value)
                    yield break;

                var routine = ESendUntilAck(
                     writer => eveWriter.Write((byte)EveCodes.MaintainHost),
                     reader =>
                     {
                         AckCodes ack = (AckCodes)socketReader.ReadByte();
                         switch (ack)
                         {
                             case AckCodes.Confirm:
                                 return;
                             case AckCodes.HostNotFound:
                                 Debug.LogWarning("Host not found");
                                 break;
                             default:
                                 Debug.LogWarning($"Unexpected ack: \"{ack}\"");
                                 return;
                         }
                         hosting.Value = false;
                     },
                     () =>
                     {
                         Debug.LogWarning("Failed to maintain host");
                         hosting.Value = false;
                     });

                while (hosting.Value && routine.MoveNext())
                    yield return null;
            }
        }
    }
}