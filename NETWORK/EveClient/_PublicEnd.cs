using _UTIL_;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public static readonly ThreadSafe<IPAddress> publicIP = new();

        //----------------------------------------------------------------------------------------------------------

        public void QueryPublicIP()
        {
            eveConn.keepAlive = true;
            eveConn.socket.selfConn.publicEnd = null;
            eveConn.channel_eve.eve_buffer.TryWrite(writer => writer.Write((byte)EveCodes.GetPublicEnd));
        }

        void OnPublicEndAck()
        {
            lock (eveConn.socket.selfConn)
            {
                IPEndPoint current = eveConn.socket.selfConn.publicEnd;
                IPEndPoint value = eveConn.socket.directReader.ReadIPEndPoint();
                publicIP.Value = value.Address;

                if (current == null || !current.Equals(value))
                    Debug.Log($"{this} Public IP: {value.ToString().Bold()}");

                eveConn.socket.selfConn.publicEnd = value;
            }
        }
    }
}