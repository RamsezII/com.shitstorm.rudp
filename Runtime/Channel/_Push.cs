using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public void PushStates()
        {
            lock (this)
                if (rudp_stream.HasData)
                {
                    if (!IsPending)
                    {
                        reliable_paquet = rudp_stream.ToReliablePaquet();
                        NextPaquet();
                    }
                    TrySendReliable();
                }
        }

        void NextPaquet()
        {
            lastSend = 0;
            attempt = 0;
            ++sendID;
        }

        void SendPaquet(in PaquetBuffer paquet)
        {
            lastSend = Util.TotalMilliseconds;
            lock (paquet.buffer)
            {
                RudpHeader.Write(paquet.buffer, paquet.offset, mask, sendID, attempt);
                conn.Send(paquet.buffer, paquet.offset, paquet.length);
            }
        }

        public void SendUnreliable(in PaquetBuffer paquet)
        {
            lock (this)
            {
                NextPaquet();
                SendPaquet(paquet);
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

                SendPaquet(reliable_paquet);

                if (attempt < byte.MaxValue)
                    ++attempt;
            }
        }
    }
}