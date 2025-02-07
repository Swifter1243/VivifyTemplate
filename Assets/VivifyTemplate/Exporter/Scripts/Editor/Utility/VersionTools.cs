using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    public static class VersionTools
    {
        public static string GetBundleFileName(BuildVersion version)
        {
            switch (version)
            {
                case BuildVersion.Windows2019: return "bundleWindows2019.vivify";
                case BuildVersion.Windows2021: return "bundleWindows2021.vivify";
                case BuildVersion.Android2021: return "bundleAndroid2021.vivify";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public static string GetVersionPrefix(BuildVersion version)
        {
            switch (version)
            {
                case BuildVersion.Windows2019: return "_windows2019";
                case BuildVersion.Windows2021: return "_windows2021";
                case BuildVersion.Android2021: return "_android2021";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public static string[] GetVersionsStrings()
        {
            return Enum.GetNames(typeof(BuildVersion));
        }

        public static string GetTempDirectory(BuildVersion version)
        {
            string path = Path.Combine(Application.temporaryCachePath, version.ToString());
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static void SetSinglePassMode(BuildVersion version)
        {
            switch (version)
            {
                case BuildVersion.Windows2019:
                    PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
                    break;
                case BuildVersion.Windows2021:
                case BuildVersion.Android2021:
                    PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }
    }
}
