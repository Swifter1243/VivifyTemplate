using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.Sockets
{
    public static class HostSocket
    {
        private const int Port = 5162;

        private static Socket _serverSocket;
        private static ManualResetEvent _accepting = new ManualResetEvent(false);

        public static bool Enabled { get; set; } = true;

        public static void Initialize()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);

            _serverSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _serverSocket.Bind(localEndPoint);
            _serverSocket.Listen(100);
            Task.Run(() =>
            {
                while (Enabled)
                {
                    // Set the event to nonsignaled state.  
                    _accepting.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.Log("Waiting for a connection...");
                    _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serverSocket);

                    // Wait until a connection is made before continuing.  
                    _accepting.WaitOne();
                }
            }).ConfigureAwait(false);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _accepting.Set();

            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                Debug.Log("Connected");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("Vivify/Socket/StartSocket")]
        private static void Start()
        {
            Initialize();
        }

        [MenuItem("Vivify/Socket/StopSocket")]
        private static void Stop()
        {
            Enabled = false;
        }
    }
}