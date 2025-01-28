﻿using System;
using System.Threading.Tasks;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public abstract class BundleBuilder
    {
        public abstract Task<BuildReport> Build(
            BuildSettings buildSettings,
            BuildAssetBundleOptions buildOptions,
            BuildVersion buildVersion,
            Logger mainLogger,
            Action<BuildTask> shaderKeywordRewriterAction
        );
    }
}
