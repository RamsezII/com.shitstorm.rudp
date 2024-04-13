using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        public static bool logUnexpectedAcks = true;

        //----------------------------------------------------------------------------------------------------------

        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (paquet)
                if (paquet.Pending)
                {
                    if (header.id == paquet.header.id)
                    {
                        paquet.stream.Position = 0;
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