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
            Debug.Log($"----- Received AddHostAck -----");
            lock (hostState)
            {
                HostCodes code = (HostCodes)socketReader.ReadByte();
                switch (code)
                {
                    case HostCodes.Added:
                        hostState._value = HostStates.Hosting;
                        Debug.Log($"Host added: {hostName}");
                        break;

                    case HostCodes.Exists:
                        hostState._value = HostStates.Hosting;
                        Debug.Log($"Host exists: {hostName}");
                        break;

                    case HostCodes.Maintained:
                        hostState._value = HostStates.Hosting;
                        Debug.Log($"Host maintained: {hostName}");
                        break;

                    case HostCodes.NotFound:
                        hostState._value = HostStates.Adding;
                        Debug.Log($"Host not found: {hostName}");
                        break;

                    case HostCodes.Removed:
                        hostState._value = HostStates.Adding;
                        Debug.Log($"Host removed: {hostName}");
                        break;

                    default:
                        Debug.LogWarning($"Received unimplemented host code: \"{code}\"");
                        break;
                }
            }
        }
    }
}