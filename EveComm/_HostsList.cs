using _UTIL_;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator EListHosts(Action<List<string>> onSuccess, Action onFailure)
        {
            ushort hostsOffset = 0;
            byte attempts = 0;
            bool done = false;
            List<string> list = new();

            void SendRequest()
            {
                NewPaquet();
                eveWriter.Write((byte)EveCodes.ListHosts);
                eveWriter.Write(hostsOffset);
                SendPaquet();
            }

            lock (onAcks)
                onAcks[EveCodes.ListHosts] = () =>
                {
                    attempts = 0;

                    ushort
                        recOffset = socketReader.ReadUInt16(),
                        hostsCount = socketReader.ReadUInt16();

                    if (hostsCount == 0)
                    {
                        Debug.Log("No public hosts");
                        done = true;
                    }

                    if (recOffset == hostsOffset)  // redundant check
                        while (conn.socket.HasNext())
                        {
                            string hostName = socketReader.ReadText();
                            list.Add(hostName);
                            ++hostsOffset;
                            Debug.Log(hostName);
                        }

                    if (hostsOffset < hostsCount)
                        SendRequest();
                    else
                        done = true;
                };

            while (true)
            {
                lock (recLock)
                    if (done)
                        break;

                if (++attempts < 10)
                {
                    SendRequest();
                    float speed = attempts switch
                    {
                        0 => 0,
                        1 => .1f,
                        2 => .2f,
                        3 => .35f,
                        4 => .5f,
                        _ => .8f,
                    };
                    WaitForSecondsRealtime wait = new(speed);
                    while (wait.MoveNext())
                        yield return null;
                }
            }

            if (done)
                onSuccess?.Invoke(list);
            else
                onFailure?.Invoke();

            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    lock (onAcks)
                        onAcks.Remove(EveCodes.ListHosts);
                },
            };
        }
    }
}