using _UTIL_;

namespace _RUDP_
{
    public partial class RudpChannel : Disposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly RudpStream states_stream;

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
            if (mask == RudpHeaderM.States)
                states_stream = new();
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
                if (!IsPending && states_stream != null && states_stream.HasData)
                {
                    paquet = states_stream.GetPaquetBuffer();
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
            onAck = null;
        }
    }
}