using System;
using System.Threading.Tasks;
using UnityEditor;
using VivifyTemplate.Exporter.Editor.Build.Structures;
namespace VivifyTemplate.Exporter.Editor.Build.Builder
{
    public class NativeBuilder : BundleBuilder
    {
        protected override Task<BuildReport> BuildInternal(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions, BuildVersion buildVersion,
            Logger mainLogger, Action<BuildTask> shaderKeywordRewriterAction)
        {
            return BuildAssetBundles.Build(buildSettings, buildOptions, buildVersion, mainLogger, shaderKeywordRewriterAction);
        }

        public override void Cancel()
        {
            // TODO: Cancellation
        }
    }
}
