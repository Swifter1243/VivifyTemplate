using System;
using System.Collections.Generic;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build.Structures
{
    [Serializable]
    public class BundleInfo
    {
        public Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();
        public Dictionary<string, string> prefabs = new Dictionary<string, string>();
        public List<string> bundleFiles = new List<string>();
        public Dictionary<string, uint> bundleCRCs = new Dictionary<string, uint>();
        public bool isCompressed = false;
    }
}
