using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public static class HostSocket
{
    const int PORT = 5162;

    private static Socket serverSocket;
    private static ManualResetEvent accepting = new ManualResetEvent(false);

    public static bool Enabled { get; set; } = true;

    public static void Initialize()
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);

        serverSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        serverSocket.Bind(localEndPoint);
        serverSocket.Listen(100);
        Task.Run(() =>
        {
            while (Enabled)
            {
                // Set the event to nonsignaled state.  
                accepting.Reset();

                // Start an asynchronous socket to listen for connections.  
                Debug.Log("Waiting for a connection...");
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);

                // Wait until a connection is made before continuing.  
                accepting.WaitOne();
            }
        }).ConfigureAwait(false);
    }

    private static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        accepting.Set();

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
}