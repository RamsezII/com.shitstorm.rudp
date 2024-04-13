using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        readonly RudpPaquet paquet = new();

        //----------------------------------------------------------------------------------------------------------

        void TrySendPaquet()
        {
            lock (paquet)
                if (paquet.Pending)
                {
                    if (paquet.attempt >= byte.MaxValue)
                    {
                        Debug.LogWarning($"{this} {nameof(TrySendPaquet)} attempt overflow for paquet: {paquet}".ToSubLog());
                        return;
                    }

                    ushort delay = paquet.attempt switch
                    {
                        0 => 0,
                        1 => 100,
                        2 => 150,
                        3 => 300,
                        4 => 600,
                        _ => 900,
                    };

                    double time = Util.TotalMilliseconds;
                    if (time - paquet.lastTime < delay)
                        return;
                    paquet.lastTime = time;

                    lock (paquet.buffer)
                    {
                        paquet.header.WriteToBuffer(paquet.buffer);
                        paquet.buffer[(int)RudpHeaderI.Attempt] = paquet.attempt;
                        conn.Send(paquet.buffer, 0, (ushort)paquet.stream.Position);
                    }

                    if (paquet.attempt < byte.MaxValue)
                        ++paquet.attempt;
                }
        }
    }
}