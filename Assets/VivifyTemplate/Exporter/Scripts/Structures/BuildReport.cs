using JetBrains.Annotations;
using UnityEditor;

namespace VivifyTemplate.Exporter.Scripts.Structures
{
    public struct BuildReport
    {
        /** This is the path to the bundle built by BuildPipeline. */
        public string BuiltBundlePath;
        /** This is the path to the bundle built by ShaderKeywordsRewriter. */
        [CanBeNull] public string FixedBundlePath;
        /** This is the path to the bundle actually cloned to the chosen output directory. */
        public string UsedBundlePath;
        /** This is the path to the bundle in the chosen output directory. */
        public string OutputBundlePath;
        public bool ShaderKeywordsFixed;
        public uint CRC;
        public BuildVersionBuildInfo BuildVersionBuildInfo;
        public BuildTarget BuildTarget;
        public BuildVersion BuildVersion;
    }
}
