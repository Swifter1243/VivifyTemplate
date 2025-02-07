using System;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class WorkingVersion
    {
        private static readonly string PlayerPrefsKey = "workingVersion";

        public static BuildVersion Value
        {
            get
            {
                string pref = UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey, null);

                if (!Enum.TryParse(pref, out BuildVersion ver))
                {
                    BuildVersion defaultVersion = BuildVersion.Windows2019;
                    UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, defaultVersion.ToString());
                    return defaultVersion;
                }

                return ver;
            }
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value.ToString());
        }
    }
}
