using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator<float> EListHosts(Action<List<string>> onListChange, Action<List<string>> onListFinal, Action onFailure)
        {
            ushort hostsOffset = 0;
            List<string> list = new();

            var eSend = ESendUntilAck(OnWriter, OnAck, onFailure);
            while (eSend.MoveNext())
                yield return eSend.Current;
            onListFinal?.Invoke(list);

            void OnWriter(BinaryWriter writer)
            {
                writer.Write((byte)EveCodes.ListHosts);
                writer.Write(hostsOffset);
            }

            void OnAck(BinaryReader socketReader)
            {
                ushort
                    recOffset = socketReader.ReadUInt16(),
                    hostsCount = socketReader.ReadUInt16();

                if (hostsCount == 0)
                {
                    Debug.Log("No public hosts");
                    onListChange?.Invoke(list);
                }

                if (recOffset == hostsOffset)  // redundant check
                {
                    while (conn.socket.HasNext())
                    {
                        string hostName = socketReader.ReadText();
                        list.Add(hostName);
                        ++hostsOffset;
                        Debug.Log(hostName);
                    }
                    onListChange?.Invoke(list);
                }

                if (hostsOffset < hostsCount)
                    eSend = ESendUntilAck(OnWriter, OnAck, onFailure);
            }
        }
    }
}