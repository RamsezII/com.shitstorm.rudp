using _UTIL_;

namespace _RUDP_
{
    public partial class RudpChannel : Disposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly RudpStream states_stream;

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
            if (mask == RudpHeaderM.States)
                states_stream = new();
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            states_stream?.Dispose();
        }
    }
}