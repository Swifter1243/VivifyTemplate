using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;

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

            _clientSocket.Connect(remoteEndPoint);
        }

        [MenuItem("Vivify/Socket/StartSocket Local Connect")]
        private static void Start()
        {
            Initialize();
        }

        [MenuItem("Vivify/Socket/StopSocket Local Connect")]
        private static void Stop()
        {
            _clientSocket.Disconnect(false);
            _clientSocket.Dispose();
        }
    }
}