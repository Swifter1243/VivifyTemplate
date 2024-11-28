using System.Diagnostics;
using System.IO;

namespace VivifyTemplate.Exporter.Scripts
{
    public static class FolderOpener
    {
        public static void OpenFolder(string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Process.Start("open", outputDirectory);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", outputDirectory);
#else
            Debug.LogWarning("This platform is not supported for opening directories.");
#endif
            }
            else
            {
                throw new FileNotFoundException($"Output directory '{path}' does not exist or is not set.");
            }
        }
    }
}
