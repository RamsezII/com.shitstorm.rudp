using _UTIL_;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        readonly ThreadSafe<bool> hosting = new();
        string hostName;
        int publicHash, privateHash;

        //--------------------------------------------------------------------------------------------------------------

        public void StartHosting(in string hostName, in int publicHash, in int privateHash)
        {
            hosting.Value = true;
            this.hostName = hostName;
            this.publicHash = publicHash;
            this.privateHash = privateHash;
            Debug.Log($"Hosting {hostName}...");
        }

        void WriteRequest_AddHost()
        {
            eveWriter.Write((byte)EveCodes.AddHost);
            eveWriter.WriteIPEnd(conn.socket.selfConn.localEnd);
            eveWriter.WriteText(hostName);
            eveWriter.Write(publicHash);
        }

        void ReceiveHolepunch()
        {
            RudpConnection hostConn = conn.socket.ReadConnection(socketReader);
            hostConn.keepAlive = true;
            Debug.Log($"Joining host confirmed ({hostConn})");
        }
    }
}