using _UTIL_;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpConnection
    {
        IEnumerator eSendFile;
        IEnumerator eReceiveFile;

        //----------------------------------------------------------------------------------------------------------

        public void SendFile()
        {
            eSendFile = ESendFile();
        }

        public void ReceiveFile()
        {
            eReceiveFile = EReceiveFile();
        }

        IEnumerator ESendFile()
        {
            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    Debug.Log("File sent");
                },
            };
            yield break;
        }

        IEnumerator EReceiveFile()
        {
            using Disposable disposable = new()
            {
                onDispose = () =>
                {
                    Debug.Log("File received");
                },
            };
            yield break;
        }
    }
}