using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public class ShouldPrettifyBundleInfo
    {
        private static readonly string PlayerPrefsKey = "shouldPrettifyBundleInfo";

        public static bool Value
        {
            get => UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKey, 1) == 1;
            set => UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
        }

        [MenuItem("Vivify/Settings/Prettify Bundle Info JSON/True")]
        private static void PrettifyBundleInfo_True() => Value = true;
        [MenuItem("Vivify/Settings/Prettify Bundle Info JSON/True", true)]
        private static bool ValidatePrettifyBundleInfo_True() => !Value;

        [MenuItem("Vivify/Settings/Prettify Bundle Info JSON/False")]
        private static void PrettifyBundleInfo_False() => Value = false;
        [MenuItem("Vivify/Settings/Prettify Bundle Info JSON/False", true)]
        private static bool ValidatePrettifyBundleInfo_False() => Value;
    }
}
