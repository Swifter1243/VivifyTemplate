using System;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class OutputDirectory
    {
        public static string Get()
        {
            if (PlayerPrefs.HasKey("bundleDir"))
            {
                return PlayerPrefs.GetString("bundleDir");
            }
			
            string outputDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
            if (outputDirectory == "")
            {
                throw new Exception("User closed the directory window.");
            }
            PlayerPrefs.SetString("bundleDir", outputDirectory);
            return outputDirectory;
        }

        [MenuItem("Vivify/Forget Output Directory")]
        private static void Forget()
        {
            PlayerPrefs.DeleteKey("bundleDir");
        }
    }
}