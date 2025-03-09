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

        public void SendAckTo(in RudpHeader header, in IPEndPoint targetEnd)
        {
            lock (ACK_BUFFER)
            {
                header.Write(ACK_BUFFER, 0);
                SendTo(ACK_BUFFER, 0, RudpHeader.HEADER_length, targetEnd);
            }
        }

#if UNITY_EDITOR
        [Obsolete("Use SendTo <buffer> <offset> <size> <IPEndPoint> instead")]
        private void SendTo(in byte[] buffer, in IPEndPoint targetEnd) => throw new NotImplementedException();
#endif
        public void SendTo(in byte[] buffer, in ushort offset, in ushort length, in IPEndPoint targetEnd)
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
                if (Util_rudp.logEmptyPaquets || Util_rudp.logAllPaquets)
                    Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} (size:{length})".ToSubLog());
            }
            else
            {
                if (length >= RudpHeader.HEADER_length)
                    if (Util_rudp.logAllPaquets)
                    {
                        RudpHeader header = RudpHeader.FromBuffer(buffer);
                        Debug.Log($"{this} {nameof(SendTo)}(rudp): {targetEnd} (header:{header}, size:{length})".ToSubLog());
                    }

                if (Util_rudp.logAllPaquets)
                    if (targetEnd.Equals(eveComm.conn.endPoint))
                        Debug.Log($"{this} {nameof(SendTo)}(eve): {targetEnd} (version:{buffer[0]}, id:{buffer[1]}, size:{length})".ToSubLog());
            }

            if (Util_rudp.logOutcomingBytes)
                Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} ({buffer.LogBytes(offset, offset + length)})".ToSubLog());

            if (length > Util_rudp.PAQUET_SIZE_BIG)
                Debug.LogWarning($"{nameof(SendTo)}->{targetEnd} ERROR: {nameof(offset)}:{offset}, {nameof(length)}: {length} (underlying buffer: {buffer.Length})");
            else
                SendTo(buffer, offset, length, SocketFlags.None, targetEnd);
        }
    }
}