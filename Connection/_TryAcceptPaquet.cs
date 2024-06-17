using UnityEngine;

namespace _RUDP_
{
    partial class RudpConnection
    {
        public bool TryAcceptPaquet(in RudpHeader header)
        {
            RudpChannel channel = null;
            if (header.mask.HasFlag(channel_files.mask))
                channel = channel_files;
            else if (header.mask.HasFlag(channel_states.mask))
                channel = channel_states;

            if (Util_rudp.logAllPaquets)
                Debug.Log($"{this} Received paquet (header:{header}, size:{socket.recLength_u})".ToSubLog());

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
            else
                // flux check
                lock (channel)
                    if (header.id < channel.recID && header.id - 25 > channel.recID)
                        return false;
                    else
                        channel.recID = header.id;

            if (header.mask == RudpHeaderM.Files)
                if (socket.HasNext())
                    if (eReceiveFile == null)
                        Debug.LogError($"{this} Received FTP paquet but no fileReceiver is set");
                    else if (!eReceiveFile.MoveNext())
                        eReceiveFile = null;

            if (socket.HasNext())
            {
                byte[] recData = socket.recBuffer_u[(int)socket.recStream_u.Position..socket.recLength_u];
                lock (states_recStream)
                    switch (header.mask)
                    {
                        case RudpHeaderM.States:
                            states_recStream.Write(recData);
                            break;
                        case RudpHeaderM.Flux:
                            socket.flux_recStream.Write(recData);
                            break;
                    }
            }

            return true;
        }
    }
}