using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        public static bool logUnexpectedAcks = true;

        //----------------------------------------------------------------------------------------------------------

        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (stream_paquet)
                if (Pending)
                {
                    if (header.id == id)
                    {
                        stream_paquet.Position = 0;
                        lastSend = 0;
                        TryPushDataIntoPaquet();
                        return true;
                    }
                    else if (logUnexpectedAcks)
                        Debug.LogWarning($"{this} Received ACK for unknown paquet: {header}");
                }
                else if (logUnexpectedAcks)
                {
                    Debug.LogWarning($"{this} Received unexpected ACK: {header}");
                    return false;
                }
            return false;
        }
    }
}