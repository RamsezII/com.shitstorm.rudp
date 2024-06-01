using _UTIL_;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public enum HostStates : byte
        {
            None,
            Adding,
            Hosting,
        }

        public enum HostCodes : byte
        {
            _none_,
            Added,
            Exists,
            Maintained,
            NotFound,
            Removed,
            _last_,
        }

        public readonly ThreadSafe<HostStates> hostState = new();
        public HostCodes hostCode;
        public string hostName;
        public int publicHash;
        float lastAddRequest;

        //----------------------------------------------------------------------------------------------------------

        void MaintainHost()
        {
            lastAddRequest = Time.unscaledTime;

            lock (hostState)
                if (hostState._value != HostStates.Hosting)
                    hostState._value = HostStates.Adding;

            eveWriter.Write((byte)EveCodes.AddHost);
            eveWriter.Write(publicHash);
            eveWriter.WriteText(hostName);
        }

        void OnAddHostAck()
        {
            hostCode = (HostCodes)eveConn.socket.recPaquetReader.ReadByte();
            hostState.Value = hostCode switch
            {
                HostCodes.Added or HostCodes.Maintained => HostStates.Hosting,
                _ => HostStates.None,
            };
        }
    }
}