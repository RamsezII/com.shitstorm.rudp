#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace _RUDP_
{
    static class Util_python
    {
        [MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(HeaderCodes_python))]
        static void HeaderCodes_python()
        {
            StringBuilder log = new();

            log.AppendLine($"class {nameof(RudpHeaderI)}(enum.Enum):");
            for (RudpHeaderI i = 0; i < RudpHeaderI._last_; i++)
                log.AppendLine($"\t{i} = {(int)i}");

            log.AppendLine($"\n\nclass {nameof(RudpHeaderB)}(enum.Enum):");
            for (RudpHeaderB i = 0; i < RudpHeaderB._last_; i++)
                log.AppendLine($"\t{i} = {(int)i}");

            log.AppendLine($"\n\nclass {nameof(RudpHeaderM)}(enum.Flag):");
            for (int i = 0; i < (int)RudpHeaderB._last_; i++)
                log.AppendLine($"\t{(RudpHeaderM)(1 << i)} = 1 << {i}");

            string message = log.ToString();
            message.WriteToClipboard();
            Debug.Log(message);
        }
    }
}
#endif