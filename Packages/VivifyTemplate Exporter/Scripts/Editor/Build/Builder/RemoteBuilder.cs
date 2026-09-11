using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build.Builder
{
    public class RemoteBuilder : BundleBuilder
    {
        protected override Task<BuildReport> BuildInternal(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions, BuildVersion buildVersion,
            Logger mainLogger, Action<BuildTask> shaderKeywordRewriterAction)
        {
            var editor = QuestPreferences.UnityEditor;
            var project = QuestPreferences.ProjectPath;
            return Task.Run(async () =>
            {
                BuildReport? report = null;
                HostSocket.Initialize(socket =>
                {
                    string payload = string.Join(";", buildSettings.OutputDirectory, buildSettings.ProjectBundle, buildSettings.ShouldExportBundleInfo, buildSettings.ShouldPrettifyBundleInfo, buildSettings.WorkingVersion, buildOptions.ToString(), buildVersion.ToString());
                    Packet.SendPacket(socket, new Packet("Build", payload));
                }, (packet, socket) =>
                {
                    switch (packet.PacketName)
                    {
                        case "Log":
                            mainLogger.LogUnformatted(packet.Payload);
                            break;
                        case "BuildReport":
                            HostSocket.Enabled = false;
                            report = JsonUtility.FromJson<BuildReport>(packet.Payload);
                            break;
                    }
                });
                await EditorWrapper.BuildProject(editor, project);
                while (!report.HasValue)
                {
                    await Task.Delay(100);
                }
                return report.Value;
            });
        }

        public override void Cancel()
        {
            // TODO: Cancellation
        }
    }
}
