using _UTIL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        readonly ThreadSafe_struct<bool> hosting = new();

        //--------------------------------------------------------------------------------------------------------------

        public IEnumerator<float> EStartHosting(string hostName, int publicHash, int privateHash, Action<bool> onSuccess) => ESendUntilAck(
            writer =>
            {
                eveWriter.Write((byte)EveCodes.AddHost);
                eveWriter.WriteIPEnd(conn.socket.selfConn.localEnd);
                eveWriter.Write(conn.socket.selfConn.is_relayed);
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

        public IEnumerator<float> EMaintainHosting()
        {
            Debug.Log("Maintaining host...".ToSubLog());
            hosting.Value = true;
            while (hosting.Value)
            {
                var wait = new WaitForSecondsRealtime(4);
                while (wait.MoveNext())
                    yield return 0;

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
                    yield return routine.Current;
            }
        }
    }
}