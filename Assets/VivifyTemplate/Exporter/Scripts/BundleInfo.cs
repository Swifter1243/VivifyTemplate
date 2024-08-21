using System;
using System.Collections.Generic;

namespace VivifyTemplate.Exporter.Scripts
{
    [Serializable]
    public class BundleInfo
    {
        public Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();
        public Dictionary<string, string> prefabs = new Dictionary<string, string>();
        public Dictionary<string, uint> bundleCRCs = new Dictionary<string, uint>();
    }
}
