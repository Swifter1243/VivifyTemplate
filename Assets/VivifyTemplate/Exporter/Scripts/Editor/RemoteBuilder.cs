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
        public override Task<BuildReport> Build(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions, BuildVersion buildVersion,
            Logger mainLogger, Action<BuildTask> shaderKeywordRewriterAction)
        {
            var editor = QuestPreferences.UnityEditor;
            var project = QuestPreferences.ProjectPath;
            return Task.Run(async () =>
            {
                HostSocket.Initialize(socket =>
                {
                    Packet.SendPacket(socket, new Packet("Build", "Build quest"));
                }, (packet, socket) =>
                {
                    Debug.Log(packet.PacketName + ": " + packet.Payload);
                });
                await EditorWrapper.BuildProject(editor, project);
                Debug.Log("Finished...");
                return new BuildReport();
            });
        }
    }
}
