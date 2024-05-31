using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        IEnumerator onAck;

        //----------------------------------------------------------------------------------------------------------

        public void SetOnAck(IEnumerator onAck)
        {
            this.onAck = onAck;
            onAck.MoveNext();
        }

        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (this)
                if (paquet != null && paquet.Length > 0)
                {
                    if (header.id == id)
                    {
                        if (mask == RudpHeaderM.States)
                            states_stream.OnCleanAfterAck((ushort)paquet.Length);

                        if (mask == RudpHeaderM.Eve)
                            eve_buffer.Flush();

                        paquet = null;

                        if (onAck != null && !onAck.MoveNext())
                            onAck = null;

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