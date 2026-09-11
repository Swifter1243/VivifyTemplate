using UnityEditor;
using VivifyTemplate.Exporter.Editor.Build;
using VivifyTemplate.Exporter.Editor.Build.Builder;
using VivifyTemplate.Exporter.Editor.Build.Structures;
using VivifyTemplate.Exporter.Editor.PlayerPrefs;
namespace VivifyTemplate.Exporter.Editor.UI
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
