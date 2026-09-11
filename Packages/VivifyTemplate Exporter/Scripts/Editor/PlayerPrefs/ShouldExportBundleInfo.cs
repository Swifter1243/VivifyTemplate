using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class ShouldExportBundleInfo
    {
        private readonly static string PlayerPrefsKey = "shouldExportAssetInfo";

        public static bool Value
        {
            get => UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKey, 1) == 1;
            set => UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
        }
    }
}
