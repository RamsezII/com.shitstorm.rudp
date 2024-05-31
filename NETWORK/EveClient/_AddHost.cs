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

            eveConn.channel_eve.eve_buffer.TryWrite(writer =>
            {
                writer.Write((byte)EveCodes.AddHost);
                writer.Write(publicHash);
                writer.WriteText(hostName);
            });
        }

        void OnAddHostAck()
        {
            hostCode = (HostCodes)eveConn.socket.directReader.ReadByte();
            hostState.Value = hostCode switch
            {
                HostCodes.Added or HostCodes.Maintained => HostStates.Hosting,
                _ => HostStates.None,
            };
        }
    }
}