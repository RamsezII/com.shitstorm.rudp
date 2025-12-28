using _ARK_;
using _UTIL_;
using System;
using UnityEngine;

namespace _RUDP_
{
    partial class RudpSocket
    {
        [Serializable]
        public class RSettings : ResourcesJSon
        {
            public byte VERSION;
        }

        [Serializable]
        public class HSettings : HomeJSon
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

        public static readonly LazyValue<RSettings> r_settings = new(() =>
        {
            ResourcesJSon.TryReadResourcesJSon(true, out RSettings value);
            Util_rudp.EMPTY_LONG[0] = value.VERSION;
            Debug.Log($"RUDP_VERSION: {value.VERSION}");
            return value;
        });

        public static HSettings h_settings;

#if UNITY_EDITOR
        const string button_prefixe = "Assets/" + nameof(_RUDP_) + "/" + nameof(RudpSocket) + ".";

        //----------------------------------------------------------------------------------------------------------

        [UnityEditor.MenuItem(button_prefixe + nameof(IncrementVersion))]
        public static void IncrementVersion()
        {
            r_settings.ForcedValue();
            ++r_settings._value.VERSION;
            r_settings._value.SaveResourcesJSon();
            r_settings.ForcedValue();
        }

        [UnityEditor.MenuItem(button_prefixe + nameof(DecrementVersion))]
        static void DecrementVersion()
        {
            r_settings.ForcedValue();
            --r_settings._value.VERSION;
            r_settings._value.SaveResourcesJSon();
            r_settings.ForcedValue();
        }

        [UnityEditor.MenuItem(button_prefixe + nameof(LoadResourcesJSon))]
        static void LoadResourcesJSon()
        {
            r_settings.ForcedValue();
        }
#endif

        static void LoadSettings(in bool log)
        {
            StaticJSon.ReadStaticJSon(out h_settings, true, log);
        }
    }
}