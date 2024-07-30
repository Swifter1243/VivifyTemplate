using System;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class WorkingVersion
    {
        public static BuildVersion Value
        {
            get
            {
                string pref = PlayerPrefs.GetString("workingVersion", null);

                if (!Enum.TryParse(pref, out BuildVersion ver))
                {
                    BuildVersion defaultVersion = BuildVersion.Windows2019;
                    PlayerPrefs.SetString("workingVersion", defaultVersion.ToString());
                    return defaultVersion;
                }

                return ver;
            }
            set => PlayerPrefs.SetString("workingVersion", value.ToString());
        }

        [MenuItem("Vivify/Settings/Set Working Version/Windows 2019")]
        private static void SetWindows2019() => Value = BuildVersion.Windows2019;

        [MenuItem("Vivify/Settings/Set Working Version/Windows 2019", true)]
        private static bool ValidateWindows2019() => Value != BuildVersion.Windows2019;

        [MenuItem("Vivify/Settings/Set Working Version/Windows 2021")]
        private static void SetWindows2021() => Value = BuildVersion.Windows2021;

        [MenuItem("Vivify/Settings/Set Working Version/Windows 2021", true)]
        private static bool ValidateWindows2021() => Value != BuildVersion.Windows2021;

        [MenuItem("Vivify/Settings/Set Working Version/Android 2019")]
        private static void SetAndroid2019() => Value = BuildVersion.Android2019;

        [MenuItem("Vivify/Settings/Set Working Version/Android 2019", true)]
        private static bool ValidateAndroid2019() => Value != BuildVersion.Android2019;

        [MenuItem("Vivify/Settings/Set Working Version/Android 2021")]
        private static void SetAndroid2021() => Value = BuildVersion.Android2021;

        [MenuItem("Vivify/Settings/Set Working Version/Android 2021", true)]
        private static bool ValidateAndroid2021() => Value != BuildVersion.Android2021;
    }
}