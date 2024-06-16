using _UTIL_;
using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public enum EveCodes : byte
    {
        _none_,
        GetPublicEndPoint,
        ListHosts,
        AddHost,
        JoinHost,
        Holepunch,
        _last_,
    }

    public enum AckCodes : byte
    {
        _none_,
        Confirm,
        Reject,
        ListHosts,
        HostAlreadyExists,
        HostNotFound,
        HostPassMismatch,
        _last_,
    }

    [Serializable]
    public partial class EveComm : IDisposable
    {
        public const byte
            VERSION = 1,
            HEADER_LENGTH = 2;

        public static readonly bool
            logEvePaquets = true;

        public readonly RudpConnection conn;

        readonly byte[] eveBuffer = new byte[Util_rudp.PAQUET_SIZE];
        readonly MemoryStream eveStream;
        readonly BinaryWriter eveWriter;
        readonly BinaryReader socketReader;

        [SerializeField] byte id, attempt;
        public readonly ThreadSafe<double> lastSend = new();
        [SerializeField] bool sendFlag;

        public byte[] GetSubPaquet() => eveBuffer[..(int)eveStream.Position];
        public override string ToString() => $"{nameof(EveComm)} {conn}";

        //----------------------------------------------------------------------------------------------------------

        public EveComm(in RudpConnection eveConn)
        {
            conn = eveConn;
            eveStream = new(eveBuffer);
            eveWriter = new(eveStream, Util_rudp.ENCODING, false);
            eveWriter.Write(VERSION);
            eveWriter.Write(id);
            socketReader = conn.socket.recReader_u;
        }

        //----------------------------------------------------------------------------------------------------------

        public void TriggerCommand(in EveCodes code)
        {
            lock (eveStream)
            {
                NewHeader();
                switch (code)
                {
                    case EveCodes.GetPublicEndPoint:
                        WriteRequest_PublicIP();
                        break;
                    case EveCodes.ListHosts:
                        hostsOffset = 0;
                        WriteRequest_ListHosts();
                        break;
                    case EveCodes.AddHost:
                        break;
                    case EveCodes.JoinHost:
                        break;
                    case EveCodes.Holepunch:
                        break;
                }
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            eveStream.Dispose();
            eveWriter.Dispose();
        }
    }
}