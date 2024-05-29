namespace _RUDP_
{
    public partial class RudpChannel
    {
        /// <summary>
        /// if no data is pending, try to push data into the paquet
        /// </summary>
        public bool TryPushDataIntoPaquet()
        {
            lock (stream_paquet)
                if (Pending)
                    TrySendPaquet();
                else
                    lock (stream_data)
                    {
                        int initialPos = (int)stream_data.Position;
                        if (initialPos > 0)
                        {
                            stream_data.Position = 0;
                            stream_paquet.Position = RudpHeader.HEADER_length;

                            lastSend = 0;
                            attempt = 0;
                            ++id;

                            while (stream_paquet.Position < RudpSocket.PAQUET_SIZE)
                            {
                                ushort length = reader_data.ReadUInt16();
                                if (stream_paquet.Position + length > RudpSocket.PAQUET_SIZE)
                                {
                                    reader_data.BaseStream.Position -= sizeof(ushort);
                                    break;
                                }
                                else
                                    reader_data.BaseStream.CopyTo(stream_paquet, length);
                            }

                            int copied_length = (int)stream_data.Position;
                            int remainingData = (int)(stream_data.Length - copied_length);

                            stream_data.Position = initialPos - copied_length;

                            if (remainingData > 0)
                            {
                                byte[] buffer = stream_data.GetBuffer();
                                System.Buffer.BlockCopy(buffer, copied_length, buffer, 0, remainingData);
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