using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastSend;
        public uint send_count, send_size;

        //----------------------------------------------------------------------------------------------------------

        public void SendAckTo(in RudpHeader header, in bool no_relay, in IPEndPoint targetEnd)
        {
            lock (ACK_BUFFER)
            {
                header.Write(ACK_BUFFER, 0);
                SendTo(ACK_BUFFER, 0, RudpHeader.HEADLEN_B, no_relay, targetEnd);
            }
        }

        public void SendTo(in byte[] buffer, in ushort offset, in ushort length, in bool no_relay, IPEndPoint targetEnd)
        {
            if (disposed.Value)
            {
                Debug.LogWarning($"Disposed socket {this} discarding pushed paquet");
                return;
            }

            if (targetEnd.Equals(selfConn.endPoint))
            {
                Debug.LogWarning($"{this} will not send to self on {{{targetEnd}}}");
                return;
            }

            lock (this)
            {
                lastSend = Util.TotalMilliseconds;
                ++send_count;
                send_size += length;
            }

            HSettings h_settings = RudpSocket.h_settings;
            if (length == 0)
            {
                if (h_settings.logEmptyPaquets || h_settings.logAllPaquets)
                    Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} (size:{length})".ToSubLog());
            }
            else
            {
                if (length >= RudpHeader.HEADLEN_B)
                    if (h_settings.logAllPaquets)
                    {
                        RudpHeader header = RudpHeader.FromBuffer(buffer);
                        Debug.Log($"{this} {nameof(SendTo)}(rudp): {targetEnd} (header:{header}, size:{length})".ToSubLog());
                    }

                if (h_settings.logAllPaquets)
                    if (targetEnd.Equals(eveComm.conn.endPoint))
                        Debug.Log($"{this} {nameof(SendTo)}(eve): {targetEnd} (version:{buffer[0]}, id:{buffer[1]}, size:{length})".ToSubLog());
            }

            if (h_settings.logOutcomingBytes)
                Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} ({buffer.LogBytes(offset, offset + length)})".ToSubLog());

            if (length > Util_rudp.PAQUET_SIZE_BIG)
                Debug.LogWarning($"{nameof(SendTo)}->{targetEnd} ERROR: {nameof(offset)}={offset}, {nameof(length)}={length} (underlying buffer: {buffer.Length})");
            else
            {
                bool relay = use_relay && !no_relay;

                if (length > 0)
                    if (relay)
                    {
                        uint ip = (uint)targetEnd.Address.Address;
                        ushort port = (ushort)targetEnd.Port;

                        // little endian
                        buffer[offset + 4] = (byte)ip;
                        buffer[offset + 5] = (byte)(ip >> 8);
                        buffer[offset + 6] = (byte)(ip >> 16);
                        buffer[offset + 7] = (byte)(ip >> 24);

                        // little endian
                        buffer[offset + 8] = (byte)port;
                        buffer[offset + 9] = (byte)(port >> 8);
                    }
                    else
                        Array.Clear(buffer, 4, 6);

                SendTo(buffer, offset, length, SocketFlags.None, relay ? Util_rudp.END_RELAY : targetEnd);
            }
        }
    }
}