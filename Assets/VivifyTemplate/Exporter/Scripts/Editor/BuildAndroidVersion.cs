using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class BuildAndroidVersion
    {
        public static bool Value
        {
            get => PlayerPrefs.GetInt("buildAndroidVersions", 1) == 1;
            set => PlayerPrefs.SetInt("buildAndroidVersions", value ? 1 : 0);
        }

        [MenuItem("Vivify/Settings/Build Android Versions/True")]
        private static void BuildAndroidVersions_True() => Value = true;
        [MenuItem("Vivify/Settings/Build Android Versions/True", true)]
        private static bool ValidateBuildAndroidVersions_True() => !Value;

        [MenuItem("Vivify/Settings/Build Android Versions/False")]
        private static void BuildAndroidVersions_False() => Value = false;
        [MenuItem("Vivify/Settings/Build Android Versions/False", true)]
        private static bool ValidateBuildAndroidVersions_False() => Value;
    }
}