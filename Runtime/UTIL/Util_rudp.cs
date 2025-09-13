using _RUDP_;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static partial class Util_rudp
{
    public const string DOMAIN_3VE = "www.shitstorm.ovh";

    public const ushort
        PORT_ARMA = 40000,
        PORT_RELAY = 44000;

    public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");

    public static readonly IPEndPoint
        END_ARMA = new(IP_3VE, PORT_ARMA),
        END_RELAY = new(IP_3VE, PORT_RELAY),
        END_LOOPBACK = new(IPAddress.Loopback, PORT_ARMA);

    //public static IPEndPoint END_ARMA => new(localIP, PORT_ARMA);

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

    public static bool IsPrivateIP(this IPAddress ip) => IsPrivateIP(ip.ToString());
    public static bool IsPrivateIP(this string ip)
    {
        var parts = ip.Split('.').Select(int.Parse).ToArray();
        return
            parts[0] == 10 ||
            (parts[0] == 192 && parts[1] == 168) ||
            (parts[0] == 172 && parts[1] >= 16 && parts[1] <= 31);
    }

    public static bool IsSameSubnet24(in IPAddress ipA, in IPAddress ipB, in bool log) => IsSameSubnet24(ipA.ToString(), ipB.ToString(), log);
    public static bool IsSameSubnet24(in string ipA, in string ipB, in bool log)
    {
        var a = ipA.Split('.');
        var b = ipB.Split('.');
        bool res = a[0] == b[0] && a[1] == b[1] && a[2] == b[2];
        if (log)
            Debug.Log($"IsSameSubnet24({ipA}, {ipB}) = {res}");
        return res;
    }
}