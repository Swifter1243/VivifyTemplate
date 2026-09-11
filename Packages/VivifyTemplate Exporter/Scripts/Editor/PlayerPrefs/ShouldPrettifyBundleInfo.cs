using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class ShouldPrettifyBundleInfo
    {
        private readonly static string PlayerPrefsKey = "shouldPrettifyBundleInfo";

        public static bool Value
        {
            get => UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKey, 1) == 1;
            set => UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
        }
    }
}
