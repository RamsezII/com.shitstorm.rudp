using _UTIL_;

namespace _RUDP_
{
    public partial class RudpChannel : Disposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly RudpStream states_stream;
        public readonly RudpStreamFlux flux_stream;

        public byte[] paquet;
        public bool IsPending => paquet != null && paquet.Length > RudpHeader.HEADER_length;

        public double lastSend;
        byte sendID, attempt;
        public byte recID;
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
                case RudpHeaderM.Flux:
                    flux_stream = new();
                    break;
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