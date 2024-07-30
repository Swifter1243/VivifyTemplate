using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public struct BuildReport
    {
        public string tempBundlePath;
        public string fixedBundlePath;
        public string outputBundlePath;
        public bool shaderKeywordsFixed;
        public uint? crc;
        public bool isAndroid;
        public BuildTarget buildTarget;
        public BuildVersion buildVersion;
    }
}