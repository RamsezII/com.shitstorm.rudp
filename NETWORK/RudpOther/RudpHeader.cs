using System;
using System.IO;

namespace _RUDP_
{
    public enum RudpHeaderI : byte
    {
        Version,
        Mask,
        ID,
        Attempt,
        _last_,
    }

    enum RudpHeaderB : byte
    {
        eve,
        reliable,
        ack,
        direct,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Eve = 1 << RudpHeaderB.eve,
        Reliable = 1 << RudpHeaderB.reliable,
        Ack = 1 << RudpHeaderB.ack,
        Direct = 1 << RudpHeaderB.direct,

        EveAck = Eve | Ack,

        /// <summary>usage: File Transfer</summary>
        ReliableDirect = Reliable | Direct,
        /// <summary>usage: States</summary>
        ReliableBuffered = Reliable,
        /// <summary>usage: Audio Stream</summary>
        UnreliableDirect = Direct,
        /// <summary>usage: Flux</summary>
        UnreliableBuffered = 0,
    }

    public readonly struct RudpHeader
    {
        public const byte HEADER_length = (byte)RudpHeaderI._last_;

        public readonly byte version;
        public readonly RudpHeaderM mask;
        public readonly byte id, attempt;
        public override string ToString() => $"{{ve:{version} ma:{{{mask}}} id:{id} at:{attempt}}}";

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in RudpHeaderM mask, in byte id) : this(Util.VERSION, mask, id, 0)
        {
        }

        RudpHeader(in byte version, in RudpHeaderM mask, in byte id, in byte attempt)
        {
            this.version = version;
            this.mask = mask;
            this.id = id;
            this.attempt = attempt;
        }

        //----------------------------------------------------------------------------------------------------------

        public static RudpHeader FromBuffer(in byte[] buffer) => new(buffer[0], (RudpHeaderM)buffer[1], buffer[2], buffer[3]);

        public static RudpHeader FromReader(in BinaryReader reader) => new(reader.ReadByte(), (RudpHeaderM)reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public void Write(in BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write((byte)mask);
            writer.Write(id);
            writer.Write(attempt);
        }

        public void WriteToBuffer(in byte[] buffer)
        {
            buffer[0] = version;
            buffer[1] = (byte)mask;
            buffer[2] = id;
            buffer[3] = attempt;
        }

        public static void Write(in byte[] buffer, in RudpHeaderM mask, in byte id, in byte attempt)
        {
            buffer[0] = Util.VERSION;
            buffer[1] = (byte)mask;
            buffer[2] = id;
            buffer[3] = attempt;
        }

        public static void Write(in BinaryWriter writer, in RudpHeaderM mask, in byte id, in byte attempt)
        {
            writer.Write(Util.VERSION);
            writer.Write((byte)mask);
            writer.Write(id);
            writer.Write(attempt);
        }
    }
}