using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public bool TryAcceptEvePaquet()
        {
            lock (recLock)
            {
                lock (lastSend)
                    Debug.Log($"Eve ping: {(Util.TotalMilliseconds - lastSend._value).MillisecondsLog()}".ToSubLog());

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
                lock (onAcks)
                    if (onAcks.TryGetValue(recCode, out var onAck))
                        onAck?.Invoke();
                    else
                    {
                        Debug.LogWarning($"Received unexpected {nameof(recCode)}: \"{recCode}\"");
                        return false;
                    }

                return true;
            }
        }
    }
}