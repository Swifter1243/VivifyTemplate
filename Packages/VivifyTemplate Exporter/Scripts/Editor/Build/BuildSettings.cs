using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build
{
    public struct BuildSettings
    {
        public static BuildSettings Snapshot()
        {
            return new BuildSettings
            {
                OutputDirectory = Editor.PlayerPrefs.OutputDirectory.Value,
                ProjectBundle = Editor.PlayerPrefs.ProjectBundle.Value,
                WorkingVersion = Editor.PlayerPrefs.WorkingVersion.Value,
                ShouldExportBundleInfo = Editor.PlayerPrefs.ShouldExportBundleInfo.Value,
                ShouldPrettifyBundleInfo = Editor.PlayerPrefs.ShouldPrettifyBundleInfo.Value
            };
        }

        public string OutputDirectory;
        public string ProjectBundle;
        public bool ShouldExportBundleInfo;
        public bool ShouldPrettifyBundleInfo;
        public BuildVersion WorkingVersion;
    }
}
