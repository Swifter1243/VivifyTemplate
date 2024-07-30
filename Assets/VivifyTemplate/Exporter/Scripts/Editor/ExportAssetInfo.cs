using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class ExportAssetInfo
    {
        public static bool Value
        {
            get => PlayerPrefs.GetInt("exportAssetInfo", 1) == 1;
            set => PlayerPrefs.SetInt("exportAssetInfo", value ? 1 : 0);
        }

        [MenuItem("Vivify/Settings/Export Asset Info/True")]
        private static void ExportAssetInfo_True() => Value = true;
        [MenuItem("Vivify/Settings/Export Asset Info/True", true)]
        private static bool ValidateExportAssetInfo_True() => !Value;

        [MenuItem("Vivify/Settings/Export Asset Info/False")]
        private static void ExportAssetInfo_False() => Value = false;
        [MenuItem("Vivify/Settings/Export Asset Info/False", true)]
        private static bool ValidateExportAssetInfo_False() => Value;
    }
}