using _RUDP_;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static partial class Util_rudp
{
    public const string DOMAIN_3VE = "www.shitstorm.ovh";

    public const ushort PORT_ARMA = 44000;

    public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");
    public static readonly IPEndPoint END_ARMA = new(IP_3VE, PORT_ARMA);
    public static readonly IPEndPoint END_LOOPBACK = new(IPAddress.Loopback, PORT_ARMA);

    public static IPAddress localIP, publicIP;

    public const ushort
        PAQUET_SIZE_SMALL = 1472,
        PAQUET_SIZE_BIG = 2 * PAQUET_SIZE_SMALL,
        DATA_SIZE_SMALL = PAQUET_SIZE_SMALL - RudpHeader.HEADER_length,
        DATA_SIZE_BIG = PAQUET_SIZE_BIG - RudpHeader.HEADER_length;

    public static readonly byte[] EMPTY_BUFFER = Array.Empty<byte>();
    public static readonly Encoding ENCODING = Encoding.UTF8;

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(new IPEndPoint(IPAddress.Parse("8.8.8.8"), 1234));
        localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
    }

    //----------------------------------------------------------------------------------------------------------

    public static int ToPassHash(this string pass) => string.IsNullOrWhiteSpace(pass) ? 0 : pass.GetHashCode();


    public static void WriteHeader(this MemoryStream stream)
    {
        for (byte i = 0; i < RudpHeader.HEADER_length; i++)
            stream.WriteByte(0);
    }

    public static void WriteIPEnd(this BinaryWriter writer, in IPEndPoint value)
    {
        writer.Write((uint)value.Address.Address);
        writer.Write((ushort)value.Port);
    }

    public static IPEndPoint ReadIPEndPoint(this BinaryReader reader)
    {
        uint address = reader.ReadUInt32();
        ushort port = reader.ReadUInt16();
        return new IPEndPoint(address, port);
    }
}