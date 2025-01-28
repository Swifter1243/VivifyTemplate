using System;
using System.Threading.Tasks;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class NativeBuilder : BundleBuilder
    {
        public override Task<BuildReport> Build(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions, BuildVersion buildVersion,
            Logger mainLogger, Action<BuildTask> shaderKeywordRewriterAction)
        {
            return BuildAssetBundles.Build(buildSettings, buildOptions, buildVersion, mainLogger, shaderKeywordRewriterAction);
        }
    }
}
