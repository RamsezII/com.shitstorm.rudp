using _ARK_;
using System;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpSocket
    {
        [Serializable]
        public class Version : JSon
        {
            public static readonly string version_file = typeof(Version).FullName + json;

#if UNITY_EDITOR
            public static readonly string
                dir_editor = Path.Combine(Application.dataPath, "Resources"),
                file_editor = Path.Combine(dir_editor, version_file);
#endif

            public byte VERSION;
        }

        [Serializable]
        public class Settings : HomeJSon
        {
            public bool
                logConnections = true,
                logIncidents = false,
                logEmptyPaquets = false,
                logKeepAlives = false,
                logAllPaquets = false,
                logOutcomingBytes = false,
                logIncomingBytes = false;
        }

        public static Version version;
        public Settings settings;

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(IncrementVersion))]
        public static void IncrementVersion()
        {
            LoadVersion();
            ++version.VERSION;
            version.Save(Version.file_editor, true);
            Debug.Log($"{nameof(IncrementVersion)}: {version.VERSION}");
        }

        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(DecrementVersion))]
        static void DecrementVersion()
        {
            LoadVersion();
            --version.VERSION;
            version.Save(Version.file_editor, true);
            Debug.Log($"{nameof(DecrementVersion)}: {version.VERSION}");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(LoadVersion))]
#endif
        public static void LoadVersion()
        {
            version ??= new();

            if (!Application.isEditor)
            {
                TextAsset text = Resources.Load<TextAsset>(Version.version_file[..^".txt".Length]);
                version = JsonUtility.FromJson<Version>(text.text);
                version.OnRead();
            }
#if UNITY_EDITOR
            else
                JSon.Read(ref version, Version.file_editor, true, false);
#endif

            Util_rudp.EMPTY_LONG[0] = version.VERSION;
            Debug.Log($"{typeof(Version).FullName}: {version.VERSION}");
        }

        [ContextMenu(nameof(SaveSettings_log))]
        public void SaveSettings_log() => OnSaveSettings(true);
        public void SaveSettings_nolog() => OnSaveSettings(false);
        protected virtual void OnSaveSettings(in bool log)
        {
            settings.SaveStaticJSon(log);
        }

        [ContextMenu(nameof(LoadSettings_log))]
        public void LoadSettings_log() => OnLoadSettings(true);
        public void LoadSettings_nolog() => OnLoadSettings(false);
        protected virtual void OnLoadSettings(in bool log)
        {
            StaticJSon.ReadStaticJSon(ref settings, true, log);
        }
    }
}