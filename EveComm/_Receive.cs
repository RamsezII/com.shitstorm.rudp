using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public bool TryAcceptEvePaquet()
        {
            lock (mainLock)
            {
                if (logEvePaquets)
                    Debug.Log($"Eve ping: {(Util.TotalMilliseconds - lastSend.Value).MillisecondsLog()}".ToSubLog());

                byte version = socketReader.ReadByte();
                byte id = socketReader.ReadByte();
                EveCodes recCode = (EveCodes)socketReader.ReadByte();

                // receive message
                if (id == 0)
                {
                    switch (recCode)
                    {
                        case EveCodes.Holepunch:
                            ReceiveHolepunch();
                            break;
                        default:
                            Debug.LogWarning($"Received wrong {nameof(recCode)}: \"{recCode}\"");
                            return false;
                    }
                    return true;
                }

                lock (eveStream)
                {
                    if (eveStream.Position <= HEADER_LENGTH || eveBuffer[1] != id)
                        return false;
                    eveStream.Position = HEADER_LENGTH;
                }

                // then its an ack
                if (onAck != null)
                {
                    onAck();
                    onAck = null;
                }
                else
                {
                    Debug.LogWarning($"Received unexpected ack: \"{recCode}\"");
                    return false;
                }

                return true;
            }
        }

        void ReceiveHolepunch()
        {
            if (hosting.Value)
            {
                RudpConnection hostConn = conn.socket.ReadConnection(socketReader);
                hostConn.keepAlive = true;
                Debug.Log($"{this} Holepunch success: {hostConn}");
            }
            else
                Debug.LogWarning("Received holepunch without hosting");
        }
    }
}