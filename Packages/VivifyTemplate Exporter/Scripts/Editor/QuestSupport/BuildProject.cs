using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;
using Logger = VivifyTemplate.Exporter.Scripts.Editor.Build.Logger;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class BuildProject
    {
        [UsedImplicitly]
        public static void Build()
        {
            var mainLogger = new Logger();
            Task<BuildReport> buildReport = null;
            RemoteSocket.Initialize((packet, socket) =>
            {
                Debug.Log(packet.PacketName + ": " + packet.Payload);
                switch (packet.PacketName)
                {
                    case "Build":
                        var payload = packet.Payload.Split(';');
                        if (payload.Length != 7)
                        {
                            RemoteSocket.Send(new Packet("Log", "Invalid payload"));
                            return;
                        }
                        var buildSettings = new BuildSettings()
                        {
                            OutputDirectory = payload[0],
                            ProjectBundle = payload[1],
                            ShouldExportBundleInfo = bool.Parse(payload[2]),
                            ShouldPrettifyBundleInfo = bool.Parse(payload[3]),
                            WorkingVersion = (BuildVersion)Enum.Parse(typeof(BuildVersion), payload[4])
                        };

                        try
                        {
                            buildReport = BuildAssetBundles.Build(buildSettings,
                                (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), payload[5]),
                                (BuildVersion)Enum.Parse(typeof(BuildVersion), payload[6]), mainLogger, null);
                        }
                        catch (Exception e)
                        {
                            mainLogger.Log(e.Message);
                        }

                        break;
                }
            });
            mainLogger.OnLog += message => RemoteSocket.Send(new Packet("Log", message));
            
            new Thread(() =>
            {
                while (true)
                {
                    if (buildReport != null && buildReport.IsCompleted)
                    {
                        RemoteSocket.Send(new Packet("BuildReport", JsonUtility.ToJson(buildReport.Result)));
                        RemoteSocket.Enabled = false;
                        UnityThread.ExecuteInUpdate(() =>
                        {
                            EditorApplication.Exit(1); 
                        });
                        break;
                    }
                }
            }).Start();
        }
    }
}
