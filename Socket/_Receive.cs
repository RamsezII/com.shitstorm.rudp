using _UTIL_;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastReceive;
        public uint receive_count, receive_size;
        readonly ThreadSafe<bool> skipNextSocketException = new(true);

        public IPEndPoint recEnd_u;
        public ushort recLength_u;

        //----------------------------------------------------------------------------------------------------------

        void ReceiveFrom(IAsyncResult aResult)
        {
            if (disposed.Value)
            {
                lock (skipNextSocketException)
                    if (skipNextSocketException._value)
                        skipNextSocketException._value = false;
                    else
                        Debug.LogWarning($"Disposed socket {this} discarded incoming paquet");
                return;
            }

            try
            {
                lock (recBuffer_u)
                {
                    lastReceive = Util.TotalMilliseconds;
                    ++receive_count;
                    EndPoint remoteEnd = endIP_any;
                    recStream_u.Position = 0;
                    recLength_u = (ushort)EndReceiveFrom(aResult, ref remoteEnd);
                    receive_size += recLength_u;

                    recEnd_u = (IPEndPoint)remoteEnd;
                    RudpConnection recConn = ToConnection(recEnd_u, out bool newConn);

                    if (Util_rudp.logAllPaquets)
                        Debug.Log($"{this} ReceivedFrom: {remoteEnd} (size:{recLength_u})".ToSubLog());
                    else if (Util_rudp.logEmptyPaquets && recLength_u == 0)
                        Debug.Log($"{this} Received empty paquet from {remoteEnd}".ToSubLog());

                    lock (recConn.lastReceive)
                    {
                        if (Util_rudp.logConnections && recConn.lastReceive._value == 0)
                            Debug.Log($"{this} holepunch: {recEnd_u}".ToSubLog());
                        recConn.lastReceive._value = Util.TotalMilliseconds;
                        recConn.keepalive_attempt.Value = 10;
                    }

                    if (recConn == eveComm.conn)
                        eveComm.TryAcceptEvePaquet();
                    else
                    {
                        if (newConn)
                        {
                            Debug.Log($"incoming connection: {recConn}".ToSubLog());
                            recConn.keepAlive = true;
                        }

                        if (recLength_u >= RudpHeader.HEADER_length)
                        {
                            RudpHeader header = RudpHeader.FromReader(recReader_u);
                            if (!recConn.TryAcceptPaquet(header))
                                Debug.LogWarning($"{recConn} {nameof(recConn.TryAcceptPaquet)}: Failed to accept paquet (header:{header}, size:{recLength_u})");
                        }

                        if (recLength_u > 0 && recLength_u < RudpHeader.HEADER_length)
                            Debug.LogWarning($"{this} Received dubious paquet from {remoteEnd} (size:{recLength_u})");
                    }

                    recEnd_u = null;
                    recConn = null;
                }
            }
            catch (SocketException e)
            {
                lock (skipNextSocketException)
                    if (skipNextSocketException._value)
                        skipNextSocketException._value = false;
                    else
                        Debug.LogWarning(e.Message());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Dispose();
                return;
            }
            BeginReceive();
        }

        void BeginReceive()
        {
            EndPoint receiveEnd = endIP_any;
            try { BeginReceiveFrom(recBuffer_u, 0, Util_rudp.PAQUET_SIZE, SocketFlags.None, ref receiveEnd, ReceiveFrom, null); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}