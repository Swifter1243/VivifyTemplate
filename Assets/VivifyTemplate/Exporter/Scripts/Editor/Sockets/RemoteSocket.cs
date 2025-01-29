using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.Sockets
{
    public static class RemoteSocket
    {
        private const int Port = 5162;

        private static Socket _clientSocket;

        public static bool Enabled { get; set; } = true;

        public static void Initialize()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, Port);

            _clientSocket = new Socket(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            _ = new Thread(() =>
            {
                while (Enabled)
                {
                    while (true)
                    {
                        if (_clientSocket.Connected)
                        {
                            Packet response = Packet.ReceivePacket(_clientSocket);
                            if (response != null)
                            {
                                Debug.Log($"Received Packet: {response.PacketName}");
                                Debug.Log($"Payload: {response.Payload}");
                            }
                        }

                        // Optional: Sleep to prevent busy looping
                        Thread.Sleep(10); // Adjust sleep duration as needed
                    }
                }
            });

            _clientSocket.Connect(remoteEndPoint);
        }
    }
}