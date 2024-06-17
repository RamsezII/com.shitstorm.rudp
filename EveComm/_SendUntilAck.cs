using _UTIL_;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    partial class EveComm
    {
        public IEnumerator ESendUntilAck(Action<BinaryWriter> onWrite, Action<BinaryReader> onAck, Action onFailure)
        {
            bool done = false;

            lock (mainLock)
                this.onAck = () =>
                {
                    done = true;
                    onAck(socketReader);
                };

            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    lock (mainLock)
                        onAck = null;
                },
            };

            byte attempt = 0;
            while (true)
            {
                lock (mainLock)
                    if (done)
                        yield break;

                if (attempt++ < 6)
                {
                    lock (eveWriter)
                    {
                        eveStream.Position = HEADER_LENGTH;
                        eveBuffer[1] = ++id == 0 ? (byte)1 : id;
                        onWrite(eveWriter);
                        lastSend._value = Util.TotalMilliseconds;
                        conn.Send(eveBuffer, 0, (ushort)eveStream.Position);
                    }

                    float delay = attempt switch
                    {
                        0 => 0,
                        1 => .1f,
                        2 => .2f,
                        3 => .35f,
                        4 => .5f,
                        _ => .8f,
                    };

                    WaitForSecondsRealtime wait = new(delay);
                    while (wait.MoveNext())
                        yield return null;
                }
                else
                {
                    WaitForSecondsRealtime wait = new(1);
                    while (wait.MoveNext())
                        yield return null;

                    lock (mainLock)
                        if (done)
                            yield break;

                    Debug.LogWarning($"/!\\ Eve failure /!\\");
                    onFailure?.Invoke();
                    yield break;
                }
            }
        }
    }
}