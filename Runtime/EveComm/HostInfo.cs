namespace _RUDP_
{
    partial class EveComm
    {
        public readonly struct HostInfos
        {
            public readonly string name;
            public readonly bool relayed;
            public override string ToString() => $"host {{ {nameof(name)}=\"{name}\", {nameof(relayed)}={relayed} }}";
            public HostInfos(in string name, in bool use_relay)
            {
                this.name = name;
                this.relayed = use_relay;
            }
        }
    }
}