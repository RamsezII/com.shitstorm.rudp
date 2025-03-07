using _RUDP_;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static partial class Util_rudp
{
    public const byte VERSION = 0;
    public const string DOMAIN_3VE = "www.shitstorm.ovh";
    public const ushort PORT_RUDP = 12345;
    public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");
    public static readonly IPEndPoint END_RUDP = new(IP_3VE, PORT_RUDP);
    public static readonly IPEndPoint END_LOOPBACK = new(IPAddress.Loopback, PORT_RUDP);
    public static IPAddress localIP, publicIP;

    public static readonly bool
        logConnections = true,
        logWarnings = false,
        logErrors = true,
        logEmptyPaquets = false,
        logKeepAlives = false,
        logAllPaquets = false,
        logOutcomingBytes = false,
        logIncomingBytes = false;

    public const ushort
        PAQUET_SIZE_SMALL = 1472,
        PAQUET_SIZE_BIG = PAQUET_SIZE_SMALL,
        DATA_SIZE_BIG = PAQUET_SIZE_BIG - RudpHeader.HEADER_length;

    public static readonly byte[] EMPTY_BUFFER = Array.Empty<byte>();
    public static readonly Encoding ENCODING = Encoding.UTF8;

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


    public static void WriteHeader(this MemoryStream stream)
    {
        for (byte i = 0; i < RudpHeader.HEADER_length; i++)
            stream.WriteByte(0);
    }

    public static int ToPassHash(this string pass) => string.IsNullOrWhiteSpace(pass) ? 0 : pass.GetHashCode();
}