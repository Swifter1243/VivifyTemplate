using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class ProjectBundle
    {
        private readonly static string PlayerPrefsKey = "projectBundle";

        public static string Value
        {
            get => UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey, "bundle");
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value);
        }
    }
}
