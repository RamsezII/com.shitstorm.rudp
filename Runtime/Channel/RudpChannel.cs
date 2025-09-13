using _UTIL_;
using System.Text;

namespace _RUDP_
{
    public readonly struct PaquetBuffer
    {
        public readonly byte[] buffer;
        public readonly ushort offset, length;

        //----------------------------------------------------------------------------------------------------------

        public PaquetBuffer(in byte[] buffer, in ushort offset, in ushort length)
        {
            this.buffer = buffer;
            this.offset = offset;
            this.length = length;
        }
    }

    public partial class RudpChannel : Disposable
    {
        public readonly RudpHeaderM mask;
        public readonly RudpConnection conn;
        public readonly RudpStream states_stream;

        public PaquetBuffer reliable_paquet;
        public bool IsPending => reliable_paquet.buffer != null && reliable_paquet.length > RudpHeader.HEADLEN_B;

        public double lastSend, ping;
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

        public void AppendStatesStatus(in StringBuilder log)
        {
            lock (this)
                log.Append($"ping: {ping:0.0} ms, ");
            states_stream.AppendStatus(log);
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            states_stream?.Dispose();
        }
    }
}