using UnityEngine;

namespace _RUDP_
{
    partial class RudpChannel
    {
        void TrySend()
        {
            lock (this)
            {
                if (attempt >= byte.MaxValue)
                {
                    Debug.LogWarning($"{this} {nameof(TrySend)} attempt overflow for paquet: {this}.{id}".ToSubLog());
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

        void SendUnreliable()
        {
            lock (this)
            {
                NextPaquet();
                SendPaquet();
                paquet = null;
            }
        }

        void SendPaquet()
        {
            lock (paquet)
            {
                RudpHeader.Write(paquet, mask, id, attempt);
                conn.Send(paquet, 0, (ushort)paquet.Length);
            }
        }
    }
}