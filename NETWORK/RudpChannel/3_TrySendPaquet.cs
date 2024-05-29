using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        void TrySendPaquet()
        {
            lock (stream_paquet)
                if (Pending)
                {
                    if (attempt >= byte.MaxValue)
                    {
                        Debug.LogWarning($"{this} {nameof(TrySendPaquet)} attempt overflow for paquet: {this}.{id}".ToSubLog());
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

                    lock (buffer_paquet)
                    {
                        RudpHeader.Write(buffer_paquet, mask, id, attempt);
                        conn.Send(buffer_paquet, 0, (ushort)stream_paquet.Position);
                    }

                    if (attempt < byte.MaxValue)
                        ++attempt;
                }
        }
    }
}