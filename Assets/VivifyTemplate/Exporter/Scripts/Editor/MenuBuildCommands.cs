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
    }
}
