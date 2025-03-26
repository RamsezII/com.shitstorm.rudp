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
            public static readonly string version_file = typeof(Version).TypeToFileName() + json;

#if UNITY_EDITOR
            public static readonly string
                dir_editor = Path.Combine(Application.dataPath, "Resources"),
                file_editor = Path.Combine(dir_editor, version_file);
#endif

            public byte VERSION;
        }

        [Serializable]
        public class Settings : JSon
        {
            public static readonly string file_name = typeof(Settings).TypeToFileName() + json;
            public static string FileDir => NUCLEOR.home_path.ForceDir().FullName;
            public static string FilePath => Path.Combine(FileDir, file_name);

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
        [SerializeField] public Settings settings;

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(IncrementVersion))]
        public static void IncrementVersion()
        {
            ++version.VERSION;
            version.Save(Version.file_editor, true);
            Debug.Log($"{nameof(IncrementVersion)}: {version.VERSION}");
        }

        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(DecrementVersion))]
        static void DecrementVersion()
        {
            --version.VERSION;
            version.Save(Version.file_editor, true);
            Debug.Log($"{nameof(DecrementVersion)}: {version.VERSION}");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadVersion()
        {
            version ??= new();

#if UNITY_EDITOR
            if (Application.isEditor)
            {
                JSon.Read(ref version, Version.file_editor, true, false);
                return;
            }
#endif

            TextAsset text = Resources.Load<TextAsset>(Version.version_file[..^".txt".Length]);
            version = JsonUtility.FromJson<Version>(text.text);
            version.OnRead();
        }

        [ContextMenu(nameof(LoadSettings))]
        void LoadSettings_logged() => LoadSettings(true);
        public void LoadSettingsNoLog() => LoadSettings(false);
        public void LoadSettings(in bool log)
        {
            settings ??= new();
            JSon.Read(ref settings, Settings.FilePath, true, log);
        }

        [ContextMenu(nameof(SaveSettings))]
        public void SaveSettings()
        {
            settings ??= new();
            settings.Save(Settings.FilePath, true);
        }
    }
}