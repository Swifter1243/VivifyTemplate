using System;
using System.Threading.Tasks;
using UnityEditor;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class RemoteBuilder : BundleBuilder
    {
        public override Task<BuildReport> Build(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions, BuildVersion buildVersion,
            Logger mainLogger, Action<BuildTask> shaderKeywordRewriterAction)
        {
            return Task.Run(async () =>
            {
                return new BuildReport();
            });
        }
    }
}
