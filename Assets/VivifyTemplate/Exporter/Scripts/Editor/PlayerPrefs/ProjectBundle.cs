using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class ProjectBundle
    {
        public static readonly string DefaultValue = "bundle";
        private readonly static string PlayerPrefsKey = "projectBundle";

        public static string Value
        {
            get => UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey, DefaultValue);
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value);
        }
    }
}
