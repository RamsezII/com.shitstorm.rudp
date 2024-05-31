#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        [MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(LogPython))]
        static void LogPython()
        {
            System.Text.StringBuilder log = new();

            log.AppendLine($"class {nameof(EveCodes)}(enum.Enum):");
            for (EveCodes code = 0; code < EveCodes._last_; code++)
                if (code > EveCodes._none_)
                    log.AppendLine($"    {code} = {(int)code}");

            log.AppendLine($"VERSION = {VERSION}");
            log.AppendLine($"PORT_RUDP = {Util_rudp.PORT_RUDP}");
            log.AppendLine($"HEADER_SIZE = {(int)HeaderI._last_}");
            log.AppendLine($"PAQUET_SIZE = {Util_rudp.PAQUET_SIZE}");
            log.AppendLine($"DATA_SIZE = {Util_rudp.PAQUET_SIZE - HEADER_LENGTH}");

            string _log = log.ToString();
            _log.WriteToClipboard();
            Debug.Log(_log);
        }
    }
}
#endif