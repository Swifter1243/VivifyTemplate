using JetBrains.Annotations;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor;

namespace VivifyTemplate.Exporter.Scripts
{
    public struct BuildReport
    {
        /** This is the path to the bundle built by BuildPipeline. */
        public string builtBundlePath;
        /** This is the path to the bundle built by ShaderKeywordsRewriter. */
        [CanBeNull] public string fixedBundlePath;
        /** This is the path to the bundle actually cloned to the chosen output directory. */
        public string usedBundlePath;
        /** This is the path to the bundle in the chosen output directory. */
        public string outputBundlePath;
        public bool shaderKeywordsFixed;
        public uint? crc;
        public bool isAndroid;
        public BuildTarget buildTarget;
        public BuildVersion buildVersion;
    }
}