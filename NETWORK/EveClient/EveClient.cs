using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public enum EveCodes : byte
        {
            _none_,
            GetPublicEnd,
            ListHosts,
            AddHost,
            JoinHost,
            _last_,
        }

        public readonly RudpConnection eveConn;
        public override string ToString() => $"{nameof(EveClient)} {eveConn?.ToString() ?? "null"}";

        //----------------------------------------------------------------------------------------------------------

        public EveClient(in RudpConnection eveConn)
        {
            this.eveConn = eveConn;
        }

        //----------------------------------------------------------------------------------------------------------

        public void QueryPublicIP()
        {
            eveConn.keepAlive = true;
            eveConn.socket.selfConn.publicEnd = null;
            eveConn.channel_direct.EnqueueData(writer => writer.Write((byte)EveCodes.GetPublicEnd));
        }

        //----------------------------------------------------------------------------------------------------------

        public void Update()
        {
            if (Time.unscaledTime > lastAddRequest + 2)
                if (hostState.Value > 0)
                    MaintainHost();
        }
    }
}