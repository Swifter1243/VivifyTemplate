using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class MenuBuildCommands
    {
        [MenuItem("Vivify/Build/Build Working Version Uncompressed _F5")]
        private static void BuildWorkingVersionUncompressed()
        {
            BuildRequest request = PlatformManager.Instance.CreateRequestFromVersion(WorkingVersion.Value);
            BuildAssetBundles.BuildSingleRequestUncompressed(request);
        }

        [MenuItem("Vivify/Build/Build All Versions Compressed")]
        private static void BuildAllVersionsCompressed()
        {
            IEnumerable<BuildVersion> versions = Enum.GetValues(typeof(BuildVersion)).OfType<BuildVersion>();
            IEnumerable<BuildRequest> requests = versions.Select(v => PlatformManager.Instance.CreateRequestFromVersion(v));
            BuildAssetBundles.BuildAllRequests(requests.ToList(), BuildAssetBundleOptions.None);
        }

        [MenuItem("Vivify/Build/Build Windows Versions Compressed")]
        private static void BuildWindowsVersionsCompressed()
        {
            BuildAssetBundles.BuildAllRequests(new List<BuildRequest>
            {
                PlatformManager.Instance.CreateRequestFromVersion(BuildVersion.Windows2019),
                PlatformManager.Instance.CreateRequestFromVersion(BuildVersion.Windows2021),
            }, BuildAssetBundleOptions.None);
        }
    }
}
