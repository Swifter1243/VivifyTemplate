using JetBrains.Annotations;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class BuildProject
    {
        [UsedImplicitly]
        public static void Build()
        {
            RemoteSocket.Initialize((packet, socket) =>
            {
                Debug.Log(packet.PacketName + ": " + packet.Payload);
                Packet.SendPacket(socket, new Packet("Building", "ques"));
            });
        }
    }
}