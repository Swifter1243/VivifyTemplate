using System;

namespace VivifyTemplate.Exporter.Scripts.Structures
{
    [Serializable]
    public struct BuildVersionBuildInfo
    {
        public bool IsAndroid;
        public bool Is2019;
        public bool NeedsShaderKeywordsFixed;
    }
}
