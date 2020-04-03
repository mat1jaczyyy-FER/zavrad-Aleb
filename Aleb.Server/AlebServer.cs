using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Aleb.Common;

namespace Aleb.Server {
    class AlebServer {
        static TcpListener server;

        static async void Handshake(TcpClient client) {
            NetworkStream stream = client.GetStream();

            Console.WriteLine($"Connection from {client.Client.RemoteEndPoint}");

            StreamReader reader = new StreamReader(client.GetStream());
            StreamWriter writer = new StreamWriter(client.GetStream());

            writer.Write($"Version {Protocol.Version}\n");
            writer.Flush();

            string response;

            try {
                response = await reader.ReadLineAsync();

            } catch {
                Console.WriteLine($"{client.Client.RemoteEndPoint} disconnected without logging in");
                client.Close();
                return;
            }

            // TODO Login
        }

        static void Main(string[] args) {
            server = new TcpListener(Protocol.Localhost, 11252);
            server.Start();

            Console.WriteLine($"Aleb Server started at {server.LocalEndpoint}");

            while (true) Handshake(server.AcceptTcpClient());
        }
    }
}
