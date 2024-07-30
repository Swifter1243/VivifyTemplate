using System;
using System.IO;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
    public static class VersionTools
    {
        public static string GetBundleFileName(BuildVersion version)
        {
            switch (version)
            {
                case BuildVersion.Windows2019: return "bundle_windows2019";
                case BuildVersion.Windows2021: return "bundle_windows2021";
                case BuildVersion.Android2019: return "bundle_android2019";
                case BuildVersion.Android2021: return "bundle_android2021";
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
                case BuildVersion.Android2019: return "_android2019";
                case BuildVersion.Android2021: return "_android2021";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public static string GetTempDirectory(BuildVersion version)
        {
            string path = Path.Combine(Application.temporaryCachePath, version.ToString());
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}