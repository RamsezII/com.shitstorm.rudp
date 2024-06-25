using Codice.Client.Common;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public void PushStates()
        {
            lock (this)
                if (states_stream.HasData)
                {
                    if (!IsPending)
                    {
                        paquet = states_stream.GetPaquetBuffer();
                        NextPaquet();
                    }
                    TrySendReliable();
                }
        }

        void NextPaquet()
        {
            lastSend = 0;
            attempt = 0;
            sendID = ++sendID == 0 ? (byte)1 : sendID;
        }

        void SendPaquet()
        {
            lastSend = Util.TotalMilliseconds;
            lock (paquet)
            {
                RudpHeader.Write(paquet, mask, sendID, attempt);
                conn.Send(paquet, 0, (ushort)paquet.Length);
            }
        }

        public void SendUnreliable(in byte[] data)
        {
            lock (this)
            {
                paquet = data;
                NextPaquet();
                SendPaquet();
                paquet = null;
            }
        }

        void TrySendReliable()
        {
            lock (this)
            {
                if (attempt >= byte.MaxValue)
                {
                    Debug.LogWarning($"{this} {nameof(TrySendReliable)} attempt overflow for paquet: {this}.{sendID}".ToSubLog());
                    return;
                }

                ushort delay = attempt switch
                {
                    0 => 0,
                    1 => 100,
                    2 => 150,
                    3 => 300,
                    4 => 600,
                    _ => 900,
                };

                double time = Util.TotalMilliseconds;
                if (time - lastSend < delay)
                    return;

                SendPaquet();

                if (attempt < byte.MaxValue)
                    ++attempt;
            }
        }
    }
}