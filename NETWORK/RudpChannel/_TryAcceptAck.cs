using System;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public Action<RudpHeader> onAck;

        //----------------------------------------------------------------------------------------------------------

        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (this)
                if (paquet != null && paquet.Length > 0)
                {
                    if (header.id == id)
                    {
                        if (mask == RudpHeaderM.States)
                        {
                            states_stream.OnCleanAfterAck((ushort)paquet.Length);
                            Push();
                        }
                        else if (mask != RudpHeaderM.Eve)
                            eve_buffer.Flush();
                        onAck?.Invoke(header);
                        onAck = null;
                        paquet = null;
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