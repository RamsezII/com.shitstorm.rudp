using _UTIL_;
using System.Collections.Generic;

namespace _RUDP_
{
    public partial class EveClient
    {
        readonly ThreadSafe<ushort> hostsOffset = new();
        public static readonly List<string> hostsList = new();
        public static readonly ThreadSafe<bool> hostsListReady = new();

        //----------------------------------------------------------------------------------------------------------

        public void RebuildHostsList()
        {
            hostsOffset.Value = 0;
            hostsListReady.Value = false;
            hostsList.Clear();
            QueryHostsList();
        }

        void QueryHostsList()
        {
            eveWriter.Write((byte)EveCodes.ListHosts);
            eveWriter.Write(hostsOffset.Value);
        }

        void OnListAck()
        {
            bool remaining = eveConn.socket.directReader.ReadBool();
            if (!remaining)
                hostsListReady.Value = true;
            else
            {
                while (eveConn.socket.HasNext())
                {
                    ++hostsOffset.Value;
                    lock (hostsList)
                        hostsList.Add(eveConn.socket.directReader.ReadText());
                }
                QueryHostsList();
            }
        }
    }
}