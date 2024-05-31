using _UTIL_;

namespace _RUDP_
{
    public partial class RudpChannel : Disposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly RudpStream states_stream;
        public readonly RudpBuffer eve_buffer;

        public byte[] paquet;
        public bool IsPending => paquet != null && paquet.Length > 0;

        public double lastSend;
        public byte id, attempt;
        public override string ToString() => $"{conn}[{mask}]";

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in RudpConnection conn, in RudpHeaderM mask)
        {
            this.conn = conn;
            this.mask = mask;

            switch (mask)
            {
                case RudpHeaderM.States:
                    states_stream = new();
                    break;
                case RudpHeaderM.Eve:
                    eve_buffer = new();
                    break;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        void NextPaquet()
        {
            lastSend = 0;
            attempt = 0;
            id = ++id == 0 ? (byte)1 : id;
        }

        public void Push()
        {
            lock (this)
            {
                if (!IsPending)
                {
                    switch (mask)
                    {
                        case RudpHeaderM.States:
                            if (states_stream.HasData)
                                paquet = states_stream.GetPaquetBuffer();
                            break;

                        case RudpHeaderM.Eve:
                            if (eve_buffer.HasData)
                                paquet = eve_buffer.GetPaquetBuffer();
                            break;

                        default:
                            return;
                    }
                    NextPaquet();
                }

                if (IsPending)
                    TrySend();
            }
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            states_stream?.Dispose();
            eve_buffer?.Dispose();
            onAck = null;
        }
    }
}