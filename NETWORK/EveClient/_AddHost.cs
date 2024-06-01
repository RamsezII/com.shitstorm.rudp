using _UTIL_;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        enum HostStates : byte
        {
            None,
            Adding,
            Hosting,
        }

        readonly ThreadSafe<HostStates> hostState = new();
        public static readonly int gameHash = Application.productName.GetHashCode();
        public string hostName;
        public int publicHash;

        //----------------------------------------------------------------------------------------------------------

        void WriteAddHostRequest()
        {
            lock (hostState)
            {
                eveWriter.Write((byte)EveCodes.AddHost);
                eveWriter.Write(gameHash);
                eveWriter.WriteText(hostName);
                eveWriter.Write(publicHash);
            }
        }

        void OnAddHostAck()
        {
            bool success = socketReader.ReadBoolean();
            hostState.Value = success ? HostStates.Hosting : HostStates.Adding;
            if (success)
                eveStream.Position = HEADER_LENGTH + 1;
        }
    }
}