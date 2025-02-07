using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;

namespace VivifyTemplate.Exporter.Scripts.Editor.Sockets
{
    public static class RemoteSocket
    {
        private const int Port = 5162;

        private static Socket _clientSocket;

        public static bool Enabled { get; set; } = true;

        public static void Initialize(Action<Packet, Socket> onPacketReceived)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, Port);

            _clientSocket = new Socket(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            new Thread(() =>
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
                                UnityThread.ExecuteInUpdate(() =>
                                {
                                    onPacketReceived?.Invoke(response, _clientSocket); 
                                });
                            }
                        }

                        Thread.Sleep(10);
                    }
                }
            }).Start();

            _clientSocket.Connect(remoteEndPoint);
        }

        public static void Send(Packet packet)
        {
            Packet.SendPacket(_clientSocket, packet);
        }
    }
}