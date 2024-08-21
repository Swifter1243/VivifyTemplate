using System;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class OutputDirectory
    {
        private static readonly string PlayerPrefsKey = "outputDirectory";

        public static string Get()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                return PlayerPrefs.GetString(PlayerPrefsKey);
            }

            string outputDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
            if (outputDirectory == "")
            {
                throw new Exception("User closed the directory window.");
            }
            PlayerPrefs.SetString(PlayerPrefsKey, outputDirectory);
            return outputDirectory;
        }

        [MenuItem("Vivify/Settings/Forget Output Directory")]
        private static void Forget()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
        }
    }
}
