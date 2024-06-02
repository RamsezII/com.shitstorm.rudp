using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    [Serializable]
    public partial class EveComm : IDisposable
    {
        public interface IUser
        {
            void OnEveOperation(in EveCodes code, in bool success, in BinaryReader reader);
        }

        public const byte
            VERSION = 1,
            HEADER_LENGTH = 1;

        public static readonly bool
            logEvePaquets = true;

        [SerializeField] EveCodes armedCode;
        [SerializeField] IUser user;

        public readonly RudpSocket socket;
        public readonly RudpConnection eveConn;
        public readonly BinaryReader socketReader;

        readonly byte[] eveBuffer;
        readonly MemoryStream eveStream;
        readonly BinaryWriter eveWriter;
        public byte[] GetPaquetBuffer() => eveBuffer[..(int)eveStream.Position];
        public override string ToString() => $"{nameof(EveComm)} {eveConn}";

        //----------------------------------------------------------------------------------------------------------

        public EveComm(in RudpConnection eveConn)
        {
            this.eveConn = eveConn;
            socket = eveConn.socket;
            socketReader = eveConn.socket.recPaquetReader;

            eveBuffer = new byte[Util_rudp.PAQUET_SIZE];
            eveStream = new(eveBuffer);
            eveWriter = new(eveStream, RudpSocket.UTF8, false);
            eveWriter.Write(VERSION);
        }

        //----------------------------------------------------------------------------------------------------------

        public void StartOperation(in IUser user, in EveCodes code)
        {
            lock (this)
            {
                this.user = user;
                armedCode = code;

                lock (eveStream)
                {
                    eveStream.Position = HEADER_LENGTH;
                    eveWriter.Write((byte)code);

                    switch (code)
                    {
                        case EveCodes.GetPublicEnd:
                            break;
                        case EveCodes.ListHosts:
                            lock (hostsLock)
                            {
                                hostsOffset = 0;
                                hostsList.Clear();
                                eveWriter.Write(hostsOffset);
                            }
                            break;
                        case EveCodes.AddHost:
                            OnWriteRequest_AddHost();
                            break;
                        case EveCodes.JoinHost:
                            break;
                        case EveCodes.Test:
                            break;
                    }
                }
            }
        }

        public void Push(in bool dontwait = false)
        {
            lock (eveStream)
                if (eveStream.Position > HEADER_LENGTH)
                    lock (eveConn.lastSend)
                        if (dontwait || Util.TotalMilliseconds > eveConn.lastSend._value + 1000)
                            eveConn.Send(eveBuffer, 0, (ushort)eveStream.Position);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            eveStream.Dispose();
            eveWriter.Dispose();
        }
    }
}