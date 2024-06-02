using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public class EveComm : IDisposable
    {
        public const byte
            VERSION = 1,
            HEADER_LENGTH = 2;

        public static readonly bool
            logEvePaquets = true;

        public readonly RudpConnection eveConn;

        readonly byte[] eveBuffer;
        readonly MemoryStream eveStream;
        readonly BinaryWriter eveWriter;

        [SerializeField] byte id;

        public Action onEveAck;

        public byte[] GetSubPaquet() => eveBuffer[..(int)eveStream.Position];
        public override string ToString() => $"{nameof(EveComm)} {eveConn}";

        //----------------------------------------------------------------------------------------------------------

        public EveComm(in RudpConnection eveConn)
        {
            this.eveConn = eveConn;
            eveBuffer = new byte[Util_rudp.PAQUET_SIZE];
            eveStream = new(eveBuffer);
            eveWriter = new(eveStream, RudpSocket.UTF8, false);
            eveWriter.Write(VERSION);
            eveWriter.Write(id);
        }

        //----------------------------------------------------------------------------------------------------------

        public void WriteAndSend(in Action<BinaryWriter> onWriter)
        {
            lock (eveStream)
            {
                eveBuffer[1] = id++;
                eveStream.Position = HEADER_LENGTH;
                onWriter(eveWriter);
                eveConn.Send(eveBuffer, 0, (ushort)eveStream.Position);
            }
        }

        public bool SendPaquetIfData()
        {
            lock (eveStream)
                if (eveStream.Position > HEADER_LENGTH)
                {
                    eveConn.Send(eveBuffer, 0, (ushort)eveStream.Position);
                    return true;
                }
            return false;
        }

        public bool TryAcceptEvePaquet()
        {
            lock (eveStream)
            {
                if (eveStream.Position <= HEADER_LENGTH || eveBuffer[1] != eveConn.socket.PAQUET_BUFFER[1])
                    return false;
                eveStream.Position = HEADER_LENGTH;
            }
            onEveAck();
            return true;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            eveStream.Dispose();
            eveWriter.Dispose();
        }
    }
}