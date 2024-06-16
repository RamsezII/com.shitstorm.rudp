using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        public void Push()
        {
            lock (this)
                switch (mask)
                {
                    case RudpHeaderM.States:
                        if (states_stream.HasData)
                        {
                            if (!IsPending)
                            {
                                this.paquet = states_stream.GetPaquetBuffer();
                                NextPaquet();
                            }
                            TrySendReliable();
                        }
                        break;

                    case RudpHeaderM.Flux:
                        if (flux_stream.TryPullPaquet(out byte[] paquet))
                        {
                            this.paquet = paquet;
                            NextPaquet();
                            SendUnreliable();
                            this.paquet = null;
                            flux_stream.CleanOldData();
                        }
                        break;
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
            lock (paquet)
            {
                RudpHeader.Write(paquet, mask, sendID, attempt);
                conn.Send(paquet, 0, (ushort)paquet.Length);
            }
        }

        public void SendUnreliable()
        {
            lock (this)
            {
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
                lastSend = time;

                SendPaquet();

                if (attempt < byte.MaxValue)
                    ++attempt;
            }
        }
    }
}