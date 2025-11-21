using _ARK_;
using _UTIL_;
using System;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpSocket
    {
        [Serializable]
        public class Version : ResourcesJSon
        {
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

        public static readonly LazyValue<Version> version = new(() =>
        {
            ResourcesJSon.TryReadResourcesJSon(true, out Version value);
            return value;
        });

        public Settings settings;

        //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(IncrementVersion))]
        public static void IncrementVersion()
        {
            LoadVersion();
            Version version = RudpSocket.version.GetValue();
            ++version.VERSION;
            version.Save();
            Debug.Log($"{nameof(IncrementVersion)}: {version.VERSION}");
        }

        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(DecrementVersion))]
        static void DecrementVersion()
        {
            LoadVersion();
            Version version = RudpSocket.version.GetValue();
            --version.VERSION;
            version.Save();
            Debug.Log($"{nameof(DecrementVersion)}: {version.VERSION}");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_RUDP_) + "/" + nameof(LoadVersion))]
#endif
        public static void LoadVersion()
        {
            Version version = RudpSocket.version.GetValue();
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