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
        direct,
        reliable,
        ack,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Eve = 1 << RudpHeaderB.eve,
        Direct = 1 << RudpHeaderB.direct,
        Reliable = 1 << RudpHeaderB.reliable,
        Ack = 1 << RudpHeaderB.ack,

        EveAck = Eve | Ack,

        Files = Reliable | Direct,
        Audio = Direct,
        States = Reliable,
        Flux = 0,
    }

    public readonly struct RudpHeader
    {
        public const byte HEADER_length = (byte)RudpHeaderI._last_;

        public readonly RudpHeaderM mask;
        public readonly byte version;
        public readonly byte id, attempt;
        public override string ToString() => $"{{ve:{version} ma:{{{mask}}} id:{id} at:{attempt}}}";

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in byte id, in RudpHeaderM mask, in byte attempt) : this(Util_rudp.VERSION, mask, id, attempt)
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

        public void WriteToBuffer(in byte[] buffer)
        {
            buffer[(int)RudpHeaderI.Version] = version;
            buffer[(int)RudpHeaderI.Mask] = (byte)mask;
            buffer[(int)RudpHeaderI.ID] = id;
            buffer[(int)RudpHeaderI.Attempt] = attempt;
        }

        public static void Write(in byte[] buffer, in RudpHeaderM mask, in byte id, in byte attempt)
        {
            buffer[(int)RudpHeaderI.Version] = Util_rudp.VERSION;
            buffer[(int)RudpHeaderI.Mask] = (byte)mask;
            buffer[(int)RudpHeaderI.ID] = id;
            buffer[(int)RudpHeaderI.Attempt] = attempt;
        }
    }
}