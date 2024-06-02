using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveComm
    {
        public static readonly IEnumerable<EveCodes> EEveCodes = Enumerable.Range(0, (int)EveCodes._last_).Select(i => (EveCodes)i);

        //----------------------------------------------------------------------------------------------------------

        public void TryAcceptEvePaquet()
        {
            lock (eveStream)
            {
                eveStream.Position = HEADER_LENGTH;

                byte version = socketReader.ReadByte();

                if (version != VERSION)
                {
                    Debug.LogError($"{eveConn} Received wrong eve version: {version}");
                    eveConn.socket.Dispose();
                    return;
                }

                EveCodes code = (EveCodes)eveConn.socket.recPaquetReader.ReadByte();

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

#if UNITY_EDITOR
                    case EveCodes.Test:
                        Debug.Log($"{eveConn} Received test eve code");
                        break;
#endif

                    default:
                        Debug.LogWarning($"{eveConn} Received unimplemented eve code: \"{code}\"");
                        break;
                }

                lock (this)
                    if (armedCode != EveCodes._none_)
                    {
                        if (armedCode != code)
                            return;
                        if (user != null)
                        {
                            user.OnEveOperation(code, true, socketReader);
                            user = null;
                        }
                        armedCode = EveCodes._none_;
                    }
            }
        }
    }
}