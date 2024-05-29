using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient : MonoBehaviour
    {
        public enum EveCodes : byte
        {
            _none_,
            Test,
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

        public void OnUpdate()
        {
            if (Time.unscaledTime > lastAddRequest + 2)
                if (hostState.Value > 0)
                    MaintainHost();
        }
    }
}