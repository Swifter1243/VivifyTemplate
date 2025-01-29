using System;
using System.Threading.Tasks;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public abstract class BundleBuilder
    {
        private Task<BuildReport> _currentBuild;

        public Task<BuildReport> Build(
            BuildSettings buildSettings,
            BuildAssetBundleOptions buildOptions,
            BuildVersion buildVersion,
            Logger mainLogger,
            Action<BuildTask> shaderKeywordRewriterAction
        )
        {
            _currentBuild = BuildInternal(buildSettings, buildOptions, buildVersion, mainLogger, shaderKeywordRewriterAction);
            return _currentBuild;
        }

        protected abstract Task<BuildReport> BuildInternal(
            BuildSettings buildSettings,
            BuildAssetBundleOptions buildOptions,
            BuildVersion buildVersion,
            Logger mainLogger,
            Action<BuildTask> shaderKeywordRewriterAction
        );

        public void Cancel()
        {
            // TODO: Cancellation
        }
    }
}
