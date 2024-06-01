using _UTIL_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public static readonly IEnumerable<EveCodes> EEveCodes = Enumerable.Range(0, (int)EveCodes._last_).Select(i => (EveCodes)i);
        public readonly ThreadSafe<Func<EveCodes, BinaryReader, bool>> onEvePaquet = new();

        //----------------------------------------------------------------------------------------------------------

        public void TryAcceptEvePaquet()
        {
            byte version = socketReader.ReadByte();

            if (version != VERSION)
            {
                Debug.LogError($"{eveConn} Received wrong eve version: {version}");
                eveConn.socket.Dispose();
                return;
            }

            eveStream.Position = HEADER_LENGTH;

            EveCodes code = (EveCodes)eveConn.socket.recPaquetReader.ReadByte();

            lock (this)
                if (armedCode != EveCodes._none_)
                {
                    if (armedCode != code)
                        return;
                    armedCode = EveCodes._none_;
                }

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

            lock (onEvePaquet)
                if (onEvePaquet._value != null)
                    if (onEvePaquet._value(code, eveConn.socket.recPaquetReader))
                        onEvePaquet._value = null;
                    else
                        Debug.LogWarning($"{eveConn} Received unexpected eve code: {code}");
        }
    }
}