using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        void ReceiveHolepunch()
        {
            RudpConnection hostConn = conn.socket.ReadConnection(socketReader);
            hostConn.keepAlive = true;
            Debug.Log($"Joining host confirmed ({hostConn})");
        }
    }
}