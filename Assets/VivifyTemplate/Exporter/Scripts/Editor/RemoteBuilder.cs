using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
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
                            var payload = packet.Payload.Split(';');
                            report = new BuildReport
                            {
                                BuiltBundlePath = payload[0],
                                FixedBundlePath = payload[1],
                                UsedBundlePath = payload[2],
                                OutputBundlePath = payload[3],
                                ShaderKeywordsFixed = bool.Parse(payload[4]),
                                CRC = uint.Parse(payload[5]),
                                BuildVersionBuildInfo = JsonUtility.FromJson<BuildVersionBuildInfo>(payload[6]),
                                BuildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), payload[7]),
                                BuildVersion = (BuildVersion)Enum.Parse(typeof(BuildVersion), payload[8])
                            };
                            break;
                    }
                });
                await EditorWrapper.BuildProject(editor, project);
                Debug.Log("Finished...");
                while (!report.HasValue)
                {
                    await Task.Delay(100);
                }
                return report.Value;
            });
        }
    }
}
