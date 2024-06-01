using _RUDP_;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static partial class Util_rudp
{
    public const byte VERSION = 0;
    public const string DOMAIN_3VE = "www.shitstorm.ovh";
    public const ushort PORT_RUDP = 12345;
    public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");
    public static readonly IPEndPoint END_RUDP = new(IP_3VE, PORT_RUDP);
    public static readonly IPEndPoint END_LOOPBACK = new(IPAddress.Loopback, PORT_RUDP);
    public static IPAddress localIP;

    public static readonly bool
        logIncidents = true,
        logEmptyPaquets = false,
        logAllPaquets = true;

    public const ushort
        PAQUET_SIZE = 1472,
        DATA_SIZE = PAQUET_SIZE - RudpHeader.HEADER_length;

    public static readonly byte[] EMPTY_BUFFER = Array.Empty<byte>();

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        netSingletons.Clear();
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(new IPEndPoint(IPAddress.Parse("8.8.8.8"), 1234));
        localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
    }

    //----------------------------------------------------------------------------------------------------------

    public static bool CheckFlags(this RudpHeaderM mask, in RudpHeaderM flags, in RudpHeaderM ignore = 0) => (mask | ignore) == (flags | ignore);
}