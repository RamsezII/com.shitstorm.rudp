using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public bool TryAcceptEvePaquet(in RudpHeader header)
        {
            if (eveConn.socket.HasNext())
            {
                EveCodes code = (EveCodes)eveConn.socket.directReader.ReadByte();
                if (header.mask.HasFlag(RudpHeaderM.Ack))
                    switch (code)
                    {
                        case EveCodes.GetPublicEnd:
                            OnPublicEndAck();
                            break;

                        case EveCodes.ListHosts:
                            OnListAck();
                            break;

                        case EveCodes.AddHost:
                            OnAddHostAck();
                            break;

                        default:
                            Debug.LogError($"{eveConn} (Ack) Received unknown eve code: {code}");
                            return false;
                    }
                else
                    switch (code)
                    {
                        default:
                            Debug.LogError($"{eveConn} (Data) Received unknown eve code:{code}");
                            return false;
                    }
            }
            return true;
        }
    }
}