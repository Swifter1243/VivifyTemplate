using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public static class RemoteSocket
{
    const int PORT = 5162;

    private static Socket clientSocket;
    private static ManualResetEvent accepting = new ManualResetEvent(false);

    public static bool Enabled { get; set; } = true;

    public static void Initialize()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);

        clientSocket = new Socket(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        clientSocket.Connect(remoteEndPoint);
    }

    [MenuItem("Vivify/StartSocket Local Connect")]
    private static void Start()
    {
        Initialize();
    }

    [MenuItem("Vivify/StopSocket Local Connect")]
    private static void Stop()
    {
        clientSocket.Disconnect(false);
        clientSocket.Dispose();
    }
}