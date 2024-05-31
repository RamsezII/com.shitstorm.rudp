using _UTIL_;
using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public enum EveCodes : byte
        {
            _none_,
            Test,
            GetPublicEnd,
            ListHosts,
            AddHost,
            JoinHost,
            _last_,
        }

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

            EveCodes code = (EveCodes)eveConn.socket.directReader.ReadByte();

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
                    lock (onEvePaquet)
                        if (onEvePaquet._value != null)
                            if (onEvePaquet._value(code, eveConn.socket.directReader))
                                onEvePaquet._value = null;
                            else
                                Debug.LogWarning($"{eveConn} Received unknown eve code: {code}");
                    break;
            }
        }
    }
}