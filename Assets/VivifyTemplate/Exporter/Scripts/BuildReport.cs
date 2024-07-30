using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor;

namespace VivifyTemplate.Exporter.Scripts
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