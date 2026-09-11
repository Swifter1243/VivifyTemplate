using System.Diagnostics;
using System.IO;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    public static class IOHelper
    {
        public static void AssertDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"The directory '{directory}' doesn't exist.");
            }
        }

        public static void WipeDirectory(string directory)
        {
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }

        public static void OpenFolder(string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Process.Start("open", path);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", path);
#else
            Debug.LogWarning("This platform is not supported for opening directories.");
#endif
            }
            else
            {
                throw new FileNotFoundException($"Output directory '{path}' does not exist or is not set.");
            }
        }

        public static bool IsUnityProject(string path)
        {
            if (!Directory.Exists(path)) return false;

            bool hasProjectSettings = Directory.Exists(Path.Combine(path, "ProjectSettings"));
            bool hasAssets = Directory.Exists(Path.Combine(path, "Assets"));
            bool hasLibrary = Directory.Exists(Path.Combine(path, "Library"));
            bool hasProjectVersion = File.Exists(Path.Combine(path, "ProjectSettings", "ProjectVersion.txt"));

            return hasProjectSettings && hasAssets && hasLibrary && hasProjectVersion;
        }
    }
}
