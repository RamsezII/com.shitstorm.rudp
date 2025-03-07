using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public bool TryAcceptAck(in RudpHeader header)
        {
            bool accepted = false;
            lock (this)
                if (paquet != null && paquet.Length > 0)
                {
                    if (header.id == sendID)
                    {
                        ping = Util.TotalMilliseconds - lastSend;
                        if (mask == RudpHeaderM.States)
                            states_stream.OnCleanAfterAck((ushort)paquet.Length);
                        paquet = null;
                        accepted = true;
                    }
                    else if (Util_rudp.logWarnings)
                        Debug.LogWarning($"{this} Received ACK for unknown paquet: {header}");
                }
                else if (Util_rudp.logWarnings)
                    Debug.LogWarning($"{this} Received unexpected ACK: {header}");

            if (accepted)
                PushStates();

            return accepted;
        }
    }
}