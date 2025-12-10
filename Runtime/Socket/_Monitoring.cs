namespace _RUDP_
{
    partial class RudpSocket
    {
        public readonly struct BandwithSnapshot
        {
            public readonly uint send_count;
            public readonly uint receive_count;
            public readonly uint send_size;
            public readonly uint receive_size;

            //----------------------------------------------------------------------------------------------------------

            public BandwithSnapshot(in uint send_count, in uint receive_count, in uint send_size, in uint receive_size)
            {
                this.send_count = send_count;
                this.receive_count = receive_count;
                this.send_size = send_size;
                this.receive_size = receive_size;
            }

            //----------------------------------------------------------------------------------------------------------

            public override string ToString() => $"{{ {nameof(send_count)}: {send_count}, {nameof(receive_count)}: {receive_count}, {nameof(send_size)}: {send_size}, {nameof(receive_size)}: {receive_size} }}";
        }

        internal uint
            send_count, receive_count,
            send_size, receive_size;

        //----------------------------------------------------------------------------------------------------------

        public BandwithSnapshot PullBandwitchSnapshot(in bool reset = true)
        {
            BandwithSnapshot snapshot;
            lock (this)
            {
                snapshot = new BandwithSnapshot(
                    send_count: send_count,
                    receive_count: receive_count,
                    send_size: send_size,
                    receive_size: receive_size
                );
                if (reset)
                    send_count = receive_count = send_size = receive_size = 0;
            }
            return snapshot;
        }
    }
}