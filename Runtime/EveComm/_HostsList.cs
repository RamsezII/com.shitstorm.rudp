using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator<float> EListHosts(Action<List<HostInfos>> onListChange, Action<List<HostInfos>> onListFinal, Action onFailure)
        {
            ushort hostsOffset = 0;
            List<HostInfos> list = new();

            var eSend = ESendUntilAck(OnWriter, OnAck, onFailure);
            while (eSend.MoveNext())
                yield return eSend.Current;
            onListFinal?.Invoke(list);

            void OnWriter(BinaryWriter writer)
            {
                writer.Write((byte)EveCodes.ListHosts);
                writer.Write(hostsOffset);
            }

            void OnAck(BinaryReader reader)
            {
                ushort
                    recOffset = reader.ReadUInt16(),
                    hostsCount = reader.ReadUInt16();

                if (hostsCount == 0)
                {
                    Debug.Log("No public hosts");
                    onListChange?.Invoke(list);
                }

                if (recOffset == hostsOffset)  // redundant check
                {
                    while (conn.socket.HasNext())
                    {
                        string name = reader.ReadText();
                        bool relayed = reader.ReadBoolean();
                        HostInfos infos = new(name, relayed);
                        list.Add(infos);
                        ++hostsOffset;
                        Debug.Log(infos);
                    }
                    onListChange?.Invoke(list);
                }

                if (hostsOffset < hostsCount)
                    eSend = ESendUntilAck(OnWriter, OnAck, onFailure);
            }
        }
    }
}