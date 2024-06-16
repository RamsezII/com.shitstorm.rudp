using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public bool TryAcceptEvePaquet()
        {
            lock (lastSend)
                Debug.Log($"Eve ping: {(Util.TotalMilliseconds - lastSend._value).MillisecondsLog()}".ToSubLog());

            byte version = socketReader.ReadByte();
            byte id = socketReader.ReadByte();
            EveCodes recCode = (EveCodes)socketReader.ReadByte();

            if (id == 0)
            {
                ReceivePaquet(recCode);
                return true;
            }

            lock (eveStream)
            {
                if (eveStream.Position <= HEADER_LENGTH || eveBuffer[1] != id)
                    return false;
                eveStream.Position = HEADER_LENGTH;
            }

            ReceiveAck(recCode);
            return true;
        }

        void ReceivePaquet(in EveCodes recCode)
        {
            switch (recCode)
            {
                default:
                    Debug.LogWarning($"Received wrong {nameof(recCode)}: \"{recCode}\"");
                    break;
            }
        }

        void ReceiveAck(in EveCodes recCode)
        {
            switch (recCode)
            {
                case EveCodes.GetPublicEndPoint:
                    OnPublicEndAck();
                    break;
                case EveCodes.ListHosts:
                    OnListAck();
                    break;
                case EveCodes.AddHost:
                    break;
                case EveCodes.JoinHost:
                    break;
                case EveCodes.Holepunch:
                    break;
                default:
                    Debug.LogWarning($"Received wrong {nameof(recCode)}: \"{recCode}\"");
                    break;
            }
        }
    }
}