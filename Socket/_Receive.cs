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

                    bool skip = false;
                    if (recLength_u > 0)
                    {
                        byte version_byte = recBuffer_u[0];
                        if (recEnd_u.Equals(Util_rudp.END_RUDP))
                        {
                            if (version_byte != EveComm.VERSION)
                            {
                                Debug.LogWarning($"[SOCKET_WARNING] Skipped a paquet from a server whose network is not the same version (received: {version_byte}, expected: {EveComm.VERSION}).");
                                skip = true;
                            }
                        }
                        else if (version_byte != version.VERSION)
                        {
                            Debug.LogWarning($"[SOCKET_WARNING] Skipped a paquet from a build whose network is not the same version (received: {version_byte}, expected: {version.VERSION}).");
                            skip = true;
                        }

                        if (skip)
                            Debug.Log($"[SOCKET_LOG] Launch the SHITLAUNCHER (shitstorm.ovh) to update your local build.");
                    }

                    if (!skip)
                    {
                        RudpConnection recConn = ToConnection(recEnd_u, out bool newConn);

                        if (settings.logAllPaquets)
                            Debug.Log($"{this} ReceivedFrom: {remoteEnd} (size:{recLength_u})".ToSubLog());
                        else if (settings.logEmptyPaquets && recLength_u == 0)
                            Debug.Log($"{this} Received empty paquet from {remoteEnd}".ToSubLog());

                        lock (recConn.lastReceive)
                        {
                            if (settings.logConnections && recConn.lastReceive._value == 0)
                                Debug.Log($"{this} holepunch: {recEnd_u}".ToSubLog());
                            recConn.lastReceive._value = Util.TotalMilliseconds;
                            recConn.keepalive_attempt.Value = 0;
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
                                    if (settings.logIncidents)
                                        Debug.LogWarning($"{recConn} {nameof(recConn.TryAcceptPaquet)}: Failed to accept paquet (header:{header}, size:{recLength_u})");
                            }

                            if (recLength_u > 0 && recLength_u < RudpHeader.HEADER_length)
                                Debug.LogWarning($"{this} Received dubious paquet from {remoteEnd} (size:{recLength_u})");
                        }

                        if (settings.logIncomingBytes)
                            Debug.Log($"{this} {nameof(ReceiveFrom)}: {recEnd_u} ({recBuffer_u.LogBytes(0, recLength_u)})".ToSubLog());

                        recConn = null;
                    }
                    recEnd_u = null;
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
            try { BeginReceiveFrom(recBuffer_u, 0, Util_rudp.PAQUET_SIZE_BIG, SocketFlags.None, ref receiveEnd, ReceiveFrom, null); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}