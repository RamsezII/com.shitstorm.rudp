using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (this)
                if (paquet != null && paquet.Length > 0)
                {
                    if (header.id == sendID)
                    {
                        if (mask == RudpHeaderM.States)
                            states_stream.OnCleanAfterAck((ushort)paquet.Length);
                        paquet = null;
                        Push();
                        return true;
                    }
                    else if (Util_rudp.logIncidents)
                        Debug.LogWarning($"{this} Received ACK for unknown paquet: {header}");
                }
                else if (Util_rudp.logIncidents)
                {
                    Debug.LogWarning($"{this} Received unexpected ACK: {header}");
                    return false;
                }
            return false;
        }
    }
}