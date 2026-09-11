using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.Sockets
{
    public static class HostSocket
    {
        private const int Port = 5162;

        private static Socket _serverSocket;
        private static ManualResetEvent _accepting = new ManualResetEvent(false);
        private static Action<Socket> _onInitialize;
        private static Action<Packet, Socket> _onPacketReceived;

        public static bool Enabled { get; set; } = true;

        public static void Initialize(Action<Socket> onInitialize, Action<Packet, Socket> onPacketReceived)
        {
            _onInitialize = onInitialize;
            _onPacketReceived = onPacketReceived;
            
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);

            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _serverSocket.Dispose();
            }

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
                    _serverSocket.BeginAccept(AcceptCallback, _serverSocket);

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

                _onInitialize?.Invoke(handler);
                new Thread(() =>
                {
                    while (Enabled)
                    {
                        while (true)
                        {
                            if (handler.Connected)
                            {
                                Packet response = Packet.ReceivePacket(handler);
                                if (response != null)
                                {
                                    _onPacketReceived?.Invoke(response, handler);
                                }
                            }

                            Thread.Sleep(10);
                        }
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}