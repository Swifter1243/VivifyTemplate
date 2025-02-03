using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.Sockets
{
    public class Packet
    {
        public string PacketName { get; private set; }
        public string Payload { get; private set; }

        public Packet(string packetName, string payload)
        {
            PacketName = packetName ?? throw new ArgumentNullException(nameof(packetName));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public byte[] Serialize()
        {
            var nameBytes = Encoding.UTF8.GetBytes(PacketName);
            var payloadBytes = Encoding.UTF8.GetBytes(Payload);

            var nameLength = nameBytes.Length;
            var payloadLength = payloadBytes.Length;

            var data = new byte[4 + nameLength + 4 + payloadLength];
            Buffer.BlockCopy(BitConverter.GetBytes(nameLength), 0, data, 0, 4);
            Buffer.BlockCopy(nameBytes, 0, data, 4, nameLength);
            Buffer.BlockCopy(BitConverter.GetBytes(payloadLength), 0, data, 4 + nameLength, 4);
            Buffer.BlockCopy(payloadBytes, 0, data, 8 + nameLength, payloadLength);

            return data;
        }

        public static Packet Deserialize(byte[] data)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("Invalid data for deserialization.");

            var nameLength = BitConverter.ToInt32(data, 0);
            if (nameLength < 0 || 4 + nameLength + 4 > data.Length)
                throw new ArgumentException("Invalid packet name length.");

            var packetName = Encoding.UTF8.GetString(data, 4, nameLength);

            var payloadStartIndex = 4 + nameLength;
            var payloadLength = BitConverter.ToInt32(data, payloadStartIndex);

            if (payloadLength < 0 || payloadStartIndex + 4 + payloadLength > data.Length)
                throw new ArgumentException("Invalid payload length.");

            var payload = Encoding.UTF8.GetString(data, payloadStartIndex + 4, payloadLength);

            return new Packet(packetName, payload);
        }
        
        public static void SendPacket(Socket socket, Packet packet)
        {
            if (socket == null || !socket.Connected)
            {
                throw new ArgumentException("Socket is not connected.");
            }

            try
            {
                byte[] data = packet.Serialize();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                socket.Send(lengthPrefix); // Send length prefix
                socket.Send(data);        // Send actual packet data
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending packet: {ex.Message}");
            }
        }

        public static Packet ReceivePacket(Socket socket)
        {
            if (socket == null || !socket.Connected)
            {
                Debug.LogError("Socket is not connected.");
            }

            try
            {
                // Read the length prefix
                byte[] lengthBuffer = new byte[4];
                int received = socket.Receive(lengthBuffer, 0, 4, SocketFlags.None);

                if (received == 0)
                {
                    return null;
                }

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (packetLength <= 0)
                {
                    Debug.LogError("Invalid packet length received.");
                    return null;
                }

                // Read the actual packet data
                byte[] packetBuffer = new byte[packetLength];
                int bytesRead = 0;

                while (bytesRead < packetLength)
                {
                    int read = socket.Receive(packetBuffer, bytesRead, packetLength - bytesRead, SocketFlags.None);
                    if (read == 0) break; // Server closed connection
                    bytesRead += read;
                }

                if (bytesRead == packetLength)
                {
                    Packet packet = Packet.Deserialize(packetBuffer);
                    return packet;
                }

                Debug.LogError("Incomplete packet received.");
                return null;
            }
            catch (Exception ex)
            {
                // Ignore "An existing connection was forcibly closed by the remote host" exception
                if (ex is SocketException socketEx && socketEx.Message.StartsWith("An existing connection"))
                {
                    return null;
                }
                Debug.LogException(ex);
                return null;
            }
        }
    }
}