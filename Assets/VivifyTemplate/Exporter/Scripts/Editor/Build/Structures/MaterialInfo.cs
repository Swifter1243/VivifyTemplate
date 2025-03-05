using System;
using System.Collections.Generic;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build.Structures
{
    [Serializable]
    public class MaterialInfo
    {
        public string path;
        public Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();
    }

    [Serializable]
    public class PropertyValue
    {
        public object value;
        public Dictionary<string, object> type;

        public PropertyValue(string type, object value)
        {
            this.value = value;
            this.type = new Dictionary<string, object>
            {
                [type] = null
            };
        }
    }
}
