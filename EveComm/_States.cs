using _UTIL_;

namespace _RUDP_
{
    public enum NetStates : byte
    {
        Offline,
        Connected,
        Hosting,
    }

    partial class EveComm
    {
        public readonly ThreadSafe<NetStates> netState = new();

        //--------------------------------------------------------------------------------------------------------------


    }
}