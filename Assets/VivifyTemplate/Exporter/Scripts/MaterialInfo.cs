﻿using System;
using System.Collections.Generic;

namespace VivifyTemplate.Exporter.Scripts
{
    [Serializable]
    public class MaterialInfo
    {
        public string path;
        public Dictionary<string, Dictionary<string, string>> properties = new Dictionary<string, Dictionary<string, string>>();
    }
}
