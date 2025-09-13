using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public interface IAudioReceiver
    {
        void OnAudioPaquet(in BinaryReader reader, in ushort paquetSize, in RudpConnection recConn);
    }

    partial class RudpConnection
    {
        public IAudioReceiver iAudioReceiver;

        //----------------------------------------------------------------------------------------------------------

        public bool TryAcceptPaquet(in RudpHeader header)
        {
            RudpChannel channel = null;
            if (header.mask.HasFlag(RudpHeaderM.Files))
                channel = channel_files;
            else if (header.mask.HasFlag(RudpHeaderM.States))
                channel = channel_states;
            else if (header.mask.HasFlag(RudpHeaderM.Audio))
                channel = channel_audio;
            else if (header.mask.HasFlag(RudpHeaderM.Flux))
                channel = channel_flux;

            if (socket.settings.logAllPaquets)
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
                    else
                    {
                        byte expectedID = (byte)(channel.recID + 1);
                        if (header.id == expectedID)
                            ++channel.recID;
                        else
                        {
                            Debug.LogWarning($"{this} Received paquet with wrong id \"{header.id}\" (expected:{expectedID})");
                            return false;
                        }
                    }

                socket.SendAckTo(new(header.id, channel.mask | RudpHeaderM.Ack, header.attempt), is_relayed, endPoint);

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
                switch (header.mask)
                {
                    case RudpHeaderM.States:
                        lock (states_recStream)
                            states_recStream.Write(recData);
                        break;

                    case RudpHeaderM.Flux:
                        lock (socket.flux_recStream)
                            socket.flux_recStream.Write(recData);
                        break;

                    case RudpHeaderM.Audio:
                        if (iAudioReceiver == null)
                        {
                            Debug.LogWarning($"{this} Received audio paquet but no {nameof(iAudioReceiver)} is set");
                            return false;
                        }
                        else
                            iAudioReceiver.OnAudioPaquet(socket.recReader_u, socket.recLength_u, this);
                        break;

                    default:
                        Debug.LogError($"{this} Received paquet with unimplemented mask \"{header.mask}\"");
                        return false;
                }
            }

            return true;
        }
    }
}