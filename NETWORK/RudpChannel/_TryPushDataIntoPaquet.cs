namespace _RUDP_
{
    public partial class RudpChannel
    {
        public bool TryPushDataIntoPaquet()
        {
            lock (paquet)
                if (paquet.Pending)
                    TrySendPaquet();
                else
                    lock (stream)
                    {
                        int initialPos = (int)stream.Position;
                        if (initialPos > 0)
                        {
                            stream.Position = 0;

                            paquet.NewData(mask, reader);

                            int hole = (int)stream.Position;
                            int remainingData = (int)(stream.Length - hole);

                            stream.Position = initialPos - hole;

                            if (remainingData > 0)
                            {
                                byte[] buffer = stream.GetBuffer();
                                System.Buffer.BlockCopy(buffer, hole, buffer, 0, remainingData);
                                stream.SetLength(remainingData);
                            }

                            TrySendPaquet();
                            return true;
                        }
                    }
            return false;
        }
    }
}