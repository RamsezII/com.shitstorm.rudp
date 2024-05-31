using System;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpConnection
    {
        public bool TryAcceptPaquet(in RudpHeader header)
        {
            RudpChannel channel = null;
            if (header.mask.HasFlag(channel_files.mask))
                channel = channel_files;
            else if (header.mask.HasFlag(channel_states.mask))
                channel = channel_states;

            if (RudpSocket.logAllPaquets)
                Debug.Log($"{this} Received paquet (header:{header}, size:{socket.reclength_u})".ToSubLog());

            if (header.mask.HasFlag(RudpHeaderM.Ack))
            {
                if (!channel.TryAcceptAck(header))
                    return false;
            }
            else if (header.mask.HasFlag(RudpHeaderM.Reliable))
            {
                bool redundant = false;
                lock (channel)
                    if (header.id == channel.id)
                        redundant = true;
                    else if (header.id == channel.id + 1)
                    {
                        if (header.id < byte.MaxValue)
                            channel.id = header.id;
                    }
                    else
                        return false;

                socket.SendAckTo(new(header.id, channel.mask | RudpHeaderM.Ack, header.attempt), endPoint);

                if (redundant || !socket.HasNext())
                    return true;
            }

            if (header.mask.HasFlag(RudpHeaderM.Eve))
                return socket.eveClient.TryAcceptEvePaquet(header);

            if (header.mask.HasFlag(RudpHeaderM.Direct))
                if (socket.HasNext())
                    if (socket.onDirectRead == null)
                        Debug.LogWarning($"{this} Received direct paquet but no {nameof(socket.onDirectRead)} is set");
                    else
                        socket.onDirectRead?.Invoke(this, header.mask, socket.directReader);

            if (socket.HasNext())
                lock (socket.bufferStream)
                {
                    if (socket.bufferStream.Position > 0)
                    {
                        int hole = (int)socket.bufferStream.Position;
                        int remaining = (int)socket.bufferStream.Length - hole;

                        byte[] buffer = socket.bufferStream.GetBuffer();
                        Buffer.BlockCopy(buffer, hole, buffer, 0, remaining);
                        socket.bufferStream.Position = 0;
                        socket.bufferStream.SetLength(remaining);
                    }

                    socket.bufferStream.Position = socket.bufferStream.Length;
                    socket.directStream.WriteTo(socket.bufferStream);
                    socket.bufferStream.Position = 0;
                }
            return true;
        }
    }
}