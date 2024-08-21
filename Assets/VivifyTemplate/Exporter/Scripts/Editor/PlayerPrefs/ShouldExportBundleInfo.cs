using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class ShouldExportBundleInfo
    {
        private static readonly string PlayerPrefsKey = "shouldExportAssetInfo";

        public static bool Value
        {
            get => UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKey, 1) == 1;
            set => UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
        }

        [MenuItem("Vivify/Settings/Export Bundle Info/True")]
        private static void ExportBundleInfo_True() => Value = true;
        [MenuItem("Vivify/Settings/Export Bundle Info/True", true)]
        private static bool ValidateExportBundleInfo_True() => !Value;

        [MenuItem("Vivify/Settings/Export Bundle Info/False")]
        private static void ExportBundleInfo_False() => Value = false;
        [MenuItem("Vivify/Settings/Export Bundle Info/False", true)]
        private static bool ValidateExportBundleInfo_False() => Value;
    }
}
