#if UNITY_EDITOR
using _RUDP_;
using UnityEditor;
using UnityEngine;

static partial class Util_rudp
{
    [MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(LogPython))]
    static void LogPython()
    {
        System.Text.StringBuilder log = new();

        log.AppendLine($"class {nameof(EveCodes)}(enum.Enum):");
        for (EveCodes code = 0; code < EveCodes._last_; code++)
            if (code > EveCodes._none_ && code != EveCodes._last_)
                log.AppendLine($"    {code} = {(int)code}");

        log.AppendLine($"class {nameof(AckCodes)}(enum.Enum):");
        for (AckCodes code = 0; code < AckCodes._last_; code++)
            if (code > AckCodes._none_ && code != AckCodes._last_)
                log.AppendLine($"    {code} = {(int)code}");

        log.AppendLine($"VERSION = {EveComm.ARMA_VERSION}");
        log.AppendLine($"PORT_RUDP = {PORT_ARMA}");
        log.AppendLine($"HEADER_SIZE = {EveComm.HEADER_LENGTH}");
        log.AppendLine($"PAQUET_SIZE = {PAQUET_SIZE_BIG}");
        log.AppendLine($"DATA_SIZE = {PAQUET_SIZE_BIG - EveComm.HEADER_LENGTH}");

        string _log = log.ToString();
        _log.WriteToClipboard();
        Debug.Log(_log);
    }
}
#endif