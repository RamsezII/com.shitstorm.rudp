namespace _RUDP_
{
    public partial class RudpChannel
    {
        /// <summary>
        /// if no data is pending, try to push data into the paquet
        /// </summary>
        public bool TryPushDataIntoPaquet()
        {
            lock (paquet)
                if (paquet.Pending)
                    TrySendPaquet();
                else
                    lock (stream_data)
                    {
                        int initialPos = (int)stream_data.Position;
                        if (initialPos > 0)
                        {
                            stream_data.Position = 0;

                            paquet.PullData();

                            int hole = (int)stream_data.Position;
                            int remainingData = (int)(stream_data.Length - hole);

                            stream_data.Position = initialPos - hole;

                            if (remainingData > 0)
                            {
                                byte[] buffer = stream_data.GetBuffer();
                                System.Buffer.BlockCopy(buffer, hole, buffer, 0, remainingData);
                                stream_data.SetLength(remainingData);
                            }

                            TrySendPaquet();
                            return true;
                        }
                    }
            return false;
        }
    }
}