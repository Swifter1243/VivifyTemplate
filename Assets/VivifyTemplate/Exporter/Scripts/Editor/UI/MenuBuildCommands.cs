using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor.Build;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Builder;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
namespace VivifyTemplate.Exporter.Scripts.Editor.UI
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
