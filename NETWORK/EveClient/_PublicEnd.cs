using UnityEngine;

namespace _RUDP_
{
    public partial class EveComm
    {
        public void QueryPublicIP()
        {
            lock (this)
                armedCode = EveCodes.GetPublicEnd;

            eveConn.socket.selfConn.publicEnd = null;
            Debug.Log($"Querying public IP...".ToSubLog());
            eveWriter.Write((byte)EveCodes.GetPublicEnd);
        }

        void OnPublicEndAck()
        {
            eveConn.socket.selfConn.publicEnd = socketReader.ReadIPEndPoint();
            Debug.Log($"Public IP: {eveConn.socket.selfConn.publicEnd.ToString().Bold()}");
        }
    }
}