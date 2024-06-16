using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        readonly object hostsLock = new();
        public List<string> hostsList = new();
        [SerializeField] ushort hostsOffset;

        //--------------------------------------------------------------------------------------------------------------

        void WriteRequest_ListHosts()
        {
            eveWriter.Write((byte)EveCodes.ListHosts);
            eveWriter.Write(hostsOffset);
        }

        void OnListAck()
        {
            ushort
                recOff = socketReader.ReadUInt16(),
                hostsCount = socketReader.ReadUInt16();

            if (hostsCount == 0)
            {
                Debug.Log("No public hosts");
                return;
            }

            if (recOff == hostsOffset)
                while (conn.socket.HasNext())
                {
                    string hostName = socketReader.ReadText();
                    hostsList.Add(hostName);
                    ++hostsOffset;
                    Debug.Log($"{nameof(hostName)}: \"{hostName}\"");
                }

            if (hostsOffset < hostsCount)
                WriteRequest_ListHosts();
        }
    }
}