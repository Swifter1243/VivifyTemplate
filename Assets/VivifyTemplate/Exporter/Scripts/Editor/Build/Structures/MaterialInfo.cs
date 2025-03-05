using System;
using System.Collections.Generic;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build.Structures
{
    [Serializable]
    public class MaterialInfo
    {
        public string path;
        public Dictionary<string, Dictionary<string, object>> properties = new Dictionary<string, Dictionary<string, object>>();
    }
}
