using System;
using System.Data;
using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public static class OutputDirectory
    {
        private readonly static string PlayerPrefsKey = "outputDirectory";

        public static string Value
        {
            get {
                if (IsSet())
                {
                    return UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey);
                }

                return SetFromExplorer();
            }
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value);
        }

        public static string SetFromExplorer()
        {
            string outputDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
            if (outputDirectory == "")
            {
                throw new NoNullAllowedException("User closed the directory window.");
            }
            UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, outputDirectory);
            return outputDirectory;
        }

        public static bool IsSet()
        {
            return UnityEngine.PlayerPrefs.HasKey(PlayerPrefsKey);
        }
    }
}
