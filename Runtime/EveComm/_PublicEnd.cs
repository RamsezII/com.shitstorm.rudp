using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        void ReadPublicEnd()
        {
            IPEndPoint publicEnd = socketReader.ReadIPEndPoint();
            conn.socket.selfConn.publicEnd = publicEnd;

            if (!publicEnd.Address.Equals(Util_rudp.publicIP))
                Debug.Log($"Public endpoint: {publicEnd}");
            Util_rudp.publicIP = publicEnd.Address;
        }

        public IEnumerator<float> EGetPublicEnd(Action<bool> onSuccess = null)
        {
            Debug.Log($"Querying public IP...".ToSubLog());
            conn.socket.selfConn.publicEnd = null;

            return ESendUntilAck(
                 writer => eveWriter.Write((byte)EveCodes.GetPublicEndPoint),
                 reader =>
                 {
                     ReadPublicEnd();
                     onSuccess?.Invoke(true);
                 },
                 () =>
                 {
                     Debug.LogWarning("Failed to get public IP");
                     onSuccess?.Invoke(false);
                 });
        }
    }
}