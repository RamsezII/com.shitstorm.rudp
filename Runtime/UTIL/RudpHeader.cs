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
        direct,
        reliable,
        ack,
        compressed,
        broadcast,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Direct = 1 << RudpHeaderB.direct,
        Reliable = 1 << RudpHeaderB.reliable,
        Ack = 1 << RudpHeaderB.ack,
        Compressed = 1 << RudpHeaderB.compressed,
        Broadcast = 1 << RudpHeaderB.broadcast,

        Files = Reliable | Direct,
        Audio = Direct,
        States = Reliable,
        Flux = 0,
    }

    public readonly struct RudpHeader
    {
        internal const byte HEADLEN_A = (byte)RudpHeaderI._last_;
        public const byte HEADLEN_B = HEADLEN_A + 4 + 2;

        public readonly RudpHeaderM mask;
        public readonly byte version;
        public readonly byte id, attempt;
        public override string ToString() => $"{{{nameof(version)}:{version}, {nameof(mask)}:{{{mask}}}, {nameof(id)}:{id}, {nameof(attempt)}:{attempt}}}";

        //----------------------------------------------------------------------------------------------------------

        public static void Prefixe(in BinaryWriter writer) => ((RudpHeader)default).Write(writer);

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in byte id, in RudpHeaderM mask, in byte attempt) : this(RudpSocket.version.VERSION, mask, id, attempt)
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

        public void Write(in byte[] buffer, in ushort offset)
        {
            buffer[(int)RudpHeaderI.Version + offset] = version;
            buffer[(int)RudpHeaderI.Mask + offset] = (byte)mask;
            buffer[(int)RudpHeaderI.ID + offset] = id;
            buffer[(int)RudpHeaderI.Attempt + offset] = attempt;
        }

        public void Write(in BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write((byte)mask);
            writer.Write(id);
            writer.Write(attempt);
        }

        public static void Write(in byte[] buffer, in ushort offset, in RudpHeaderM mask, in byte id, in byte attempt)
        {
            buffer[(int)RudpHeaderI.Version + offset] = RudpSocket.version.VERSION;
            buffer[(int)RudpHeaderI.Mask + offset] = (byte)mask;
            buffer[(int)RudpHeaderI.ID + offset] = id;
            buffer[(int)RudpHeaderI.Attempt + offset] = attempt;
        }
    }
}