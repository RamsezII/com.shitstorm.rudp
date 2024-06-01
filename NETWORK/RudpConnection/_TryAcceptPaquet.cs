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

            if (Util_rudp.logAllPaquets)
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
                    if (header.id == channel.recID)
                        redundant = true;
                    else if (header.id == channel.recID + 1)
                        ++channel.recID;
                    else
                        return false;

                socket.SendAckTo(new(header.id, channel.mask | RudpHeaderM.Ack, header.attempt), endPoint);

                if (redundant || !socket.HasNext())
                    return true;
            }

            if (header.mask == RudpHeaderM.Files)
                if (socket.HasNext())
                    if (eReceiveFile == null)
                        Debug.LogError($"{this} Received FTP paquet but no fileReceiver is set");
                    else if (!eReceiveFile.MoveNext())
                        eReceiveFile = null;

            if (socket.HasNext())
                lock (socket.recDataStream)
                {
                    if (socket.recDataStream.Position > 0)
                    {
                        int hole = (int)socket.recDataStream.Position;
                        int remaining = (int)socket.recDataStream.Length - hole;

                        byte[] buffer = socket.recDataStream.GetBuffer();
                        Buffer.BlockCopy(buffer, hole, buffer, 0, remaining);
                        socket.recDataStream.Position = 0;
                        socket.recDataStream.SetLength(remaining);
                    }

                    socket.recDataStream.Position = socket.recDataStream.Length;
                    socket.recPaquetStream.WriteTo(socket.recDataStream);
                    socket.recDataStream.Position = 0;
                }
            return true;
        }
    }
}