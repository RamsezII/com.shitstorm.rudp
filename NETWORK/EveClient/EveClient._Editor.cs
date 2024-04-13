#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        [MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(EveCodes_python))]
        static void EveCodes_python()
        {
            StringBuilder log = new();

            log.AppendLine($"class {nameof(EveCodes)}(enum.Enum):");
            for (EveCodes i = 1 + EveCodes._none_; i < EveCodes._last_; i++)
                log.AppendLine($"\t{i} = {(int)i}");

            string message = log.ToString();
            message.WriteToClipboard();
            Debug.Log(message);
        }
    }
}
#endif