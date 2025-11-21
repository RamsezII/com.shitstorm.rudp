using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public bool TryAcceptAck(in RudpHeader header)
        {
            bool accepted = false;
            lock (this)
                if (reliable_paquet.buffer != null)
                {
                    if (header.id == sendID)
                    {
                        ping = Util.TotalMilliseconds - lastSend;
                        if (mask == RudpHeaderM.States)
                            states_stream.OnCleanAfterAck(reliable_paquet.length);
                        reliable_paquet = default;
                        accepted = true;
                    }
                    else if (RudpSocket.h_settings.logIncidents)
                        Debug.LogWarning($"{this} Received ACK for unknown paquet: {header}");
                }
                else if (RudpSocket.h_settings.logIncidents)
                    Debug.LogWarning($"{this} Received unexpected ACK: {header}");

            if (accepted)
                PushStates();

            return accepted;
        }
    }
}