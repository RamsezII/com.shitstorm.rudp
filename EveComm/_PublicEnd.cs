using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public void WriteRequest_PublicIP()
        {
            eveWriter.Write((byte)EveCodes.GetPublicEndPoint);
            Debug.Log($"Querying public IP...".ToSubLog());
            conn.socket.selfConn.publicEnd = null;
        }

        void OnPublicEndAck()
        {
            conn.socket.selfConn.publicEnd = socketReader.ReadIPEndPoint();
            if (!conn.socket.selfConn.publicEnd.Address.Equals(Util_rudp.publicIP))
                Debug.Log($"Public IP: {conn.socket.selfConn.publicEnd.Address}");
            Util_rudp.publicIP = conn.socket.selfConn.publicEnd.Address;
        }
    }
}