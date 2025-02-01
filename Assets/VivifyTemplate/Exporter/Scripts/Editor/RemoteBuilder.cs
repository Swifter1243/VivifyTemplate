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
                    Debug.Log(packet.PacketName + ": " + packet.Payload);
                    switch (packet.PacketName)
                    {
                        case "Log":
                            mainLogger.LogUnformatted(packet.Payload);
                            break;
                        case "BuildReport":
                            report = JsonUtility.FromJson<BuildReport>(packet.Payload);
                            break;
                    }
                });
                await EditorWrapper.BuildProject(editor, project);
                Debug.Log("Finished...");
                while (!report.HasValue)
                {
                    await Task.Delay(100);
                }

                HostSocket.Enabled = false;
                return report.Value;
            });
        }
    }
}
