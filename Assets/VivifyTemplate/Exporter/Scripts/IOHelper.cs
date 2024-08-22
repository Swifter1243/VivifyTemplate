using System.IO;

namespace VivifyTemplate.Exporter.Scripts
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
    }
}
