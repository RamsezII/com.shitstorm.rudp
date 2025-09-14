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

        public void SendAckTo(in RudpHeader header, in bool use_relay, in IPEndPoint targetEnd)
        {
            lock (ACK_BUFFER)
            {
                header.Write(ACK_BUFFER, 0);
                SendTo(ACK_BUFFER, 0, RudpHeader.HEADLEN_B, use_relay, false, targetEnd);
            }
        }

#if UNITY_EDITOR
        [Obsolete("Use SendTo <buffer> <offset> <size> <IPEndPoint> instead")]
        private void SendTo(in byte[] buffer, in IPEndPoint targetEnd) => throw new NotImplementedException();
#endif
        public void SendTo(in byte[] buffer, in ushort offset, in ushort length, in bool request_relay, in bool force_no_relay, in IPEndPoint targetEnd)
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

            if (length == 0)
            {
                if (settings.logEmptyPaquets || settings.logAllPaquets)
                    Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} (size:{length})".ToSubLog());
            }
            else
            {
                if (length >= RudpHeader.HEADLEN_B)
                    if (settings.logAllPaquets)
                    {
                        RudpHeader header = RudpHeader.FromBuffer(buffer);
                        Debug.Log($"{this} {nameof(SendTo)}(rudp): {targetEnd} (header:{header}, size:{length})".ToSubLog());
                    }

                if (settings.logAllPaquets)
                    if (targetEnd.Equals(eveComm.conn.endPoint))
                        Debug.Log($"{this} {nameof(SendTo)}(eve): {targetEnd} (version:{buffer[0]}, id:{buffer[1]}, size:{length})".ToSubLog());
            }

            if (settings.logOutcomingBytes)
                Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} ({buffer.LogBytes(offset, offset + length)})".ToSubLog());

            if (length > Util_rudp.PAQUET_SIZE_BIG)
                Debug.LogWarning($"{nameof(SendTo)}->{targetEnd} ERROR: {nameof(offset)}={offset}, {nameof(length)}={length} (underlying buffer: {buffer.Length})");
            else
            {
                if (length > 0)
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

                if ((request_relay || use_relay) && !force_no_relay)
                    SendTo(buffer, offset, length, SocketFlags.None, Util_rudp.END_RELAY);
                else
                    SendTo(buffer, offset, length, SocketFlags.None, targetEnd);
            }
        }
    }
}