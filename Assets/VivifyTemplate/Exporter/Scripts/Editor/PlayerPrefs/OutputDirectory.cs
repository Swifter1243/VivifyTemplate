using System;
using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class OutputDirectory
    {
        private static readonly string PlayerPrefsKey = "outputDirectory";

        public static string Get()
        {
            if (UnityEngine.PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                return UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey);
            }

            string outputDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
            if (outputDirectory == "")
            {
                throw new Exception("User closed the directory window.");
            }
            UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, outputDirectory);
            return outputDirectory;
        }

        [MenuItem("Vivify/Settings/Forget Output Directory")]
        private static void Forget()
        {
            UnityEngine.PlayerPrefs.DeleteKey(PlayerPrefsKey);
        }
    }
}
