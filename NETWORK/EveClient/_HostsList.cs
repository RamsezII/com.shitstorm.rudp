using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        readonly object hostsLock = new();
        public List<string> hostsList = new();
        [SerializeField] ushort hostsOffset;

        //----------------------------------------------------------------------------------------------------------

        void OnListAck()
        {
            ushort
                recOff = socketReader.ReadUInt16(),
                hostsCount = socketReader.ReadUInt16();

            if (recOff == hostsOffset)
                while (socket.HasNext())
                {
                    string hostName = socketReader.ReadText();
                    hostsList.Add(hostName);
                    ++hostsOffset;
                    Debug.Log($"Received host: \"{hostName}\"");
                }

            if (hostsOffset < hostsCount)
            {
                eveWriter.Write((byte)EveCodes.ListHosts);
                eveWriter.Write(hostsOffset);
                Push(true);
            }
        }
    }
}