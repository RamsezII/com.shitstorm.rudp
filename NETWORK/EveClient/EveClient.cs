using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient : IDisposable
    {
        enum HeaderI : byte
        {
            version,
            _last_
        }

        public const byte
            VERSION = 1,
            HEADER_LENGTH = (byte)HeaderI._last_;

        public readonly RudpConnection eveConn;
        public readonly BinaryReader socketReader;

        bool pending;
        public byte[] GetPaquetBuffer() => eveBuffer[..(int)eveStream.Position];
        public override string ToString() => $"{nameof(EveClient)} {eveConn}";

        //----------------------------------------------------------------------------------------------------------

        public EveClient(in RudpConnection eveConn)
        {
            this.eveConn = eveConn;
            socketReader = eveConn.socket.directReader;

            eveBuffer = new byte[Util_rudp.PAQUET_SIZE];
            eveStream = new(eveBuffer);
            eveWriter = new(eveStream, RudpSocket.UTF8, false);

            for (HeaderI code = 0; code < HeaderI._last_; code++)
                switch (code)
                {
                    case HeaderI.version:
                        eveWriter.Write(VERSION);
                        break;
                    default:
                        eveWriter.Write((byte)0);
                        break;
                }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Push()
        {
            if (pending)
                if (eveStream.Position <= HEADER_LENGTH)
                    pending = false;
                else
                    lock (eveConn.lastSend)
                        if (Util.TotalMilliseconds > eveConn.lastSend._value + 350)
                            eveConn.Send(eveBuffer, 0, (ushort)eveStream.Position);

            if (Time.unscaledTime > lastAddRequest + 2)
                if (hostState.Value > 0)
                    MaintainHost();
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            eveStream.Dispose();
            eveWriter.Dispose();
        }
    }
}