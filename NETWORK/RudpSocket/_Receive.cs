using _UTIL_;
using System;
using System.IO;
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
        public ushort reclength_u;

        public readonly MemoryStream directStream, bufferStream;
        public readonly BinaryReader directReader, bufferReader;
        public bool HasNext() => directStream.Position < reclength_u;

        public readonly ThreadSafe<Action<RudpConnection, RudpHeaderM, BinaryReader>> onDirectRead = new();

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
                lock (PAQUET_BUFFER)
                {
                    lastReceive = Util.TotalMilliseconds;
                    ++receive_count;
                    EndPoint remoteEnd = endIP_any;
                    directStream.Position = 0;
                    reclength_u = (ushort)EndReceiveFrom(aResult, ref remoteEnd);
                    receive_size += reclength_u;

                    recEnd_u = (IPEndPoint)remoteEnd;
                    RudpConnection recConn = ToConnection(recEnd_u, out bool newConn);

                    if (newConn)
                        Debug.Log($"incoming connection: {recConn}".ToSubLog());

                    recConn.lastReceive.Value = Util.TotalMilliseconds;

                    if (reclength_u >= RudpHeader.HEADER_length)
                    {
                        RudpHeader header = RudpHeader.FromReader(directReader);
                        if (!recConn.TryAcceptPaquet(header))
                            Debug.LogError($"{recConn} {nameof(recConn.TryAcceptPaquet)}: Failed to accept paquet (header:{header}, size:{reclength_u})");
                    }
                    else if (logEmptyPaquets)
                        Debug.Log($"{this} Received empty paquet from {remoteEnd}".ToSubLog());
                    else if (reclength_u > 0)
                        Debug.LogWarning($"{this} Received dubious paquet from {remoteEnd} (size:{reclength_u})");

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
            try { BeginReceiveFrom(PAQUET_BUFFER, 0, PAQUET_SIZE, SocketFlags.None, ref receiveEnd, ReceiveFrom, null); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}