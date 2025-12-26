using _UTIL_;
using System.Collections;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpConnection
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
            using Disposable disposable = new("sending_file")
            {
                onDispose = () =>
                {
                },
            };
            yield break;
        }

        IEnumerator EReceiveFile()
        {
            using Disposable disposable = new("receiving_file")
            {
                onDispose = () =>
                {
                },
            };
            yield break;
        }
    }
}