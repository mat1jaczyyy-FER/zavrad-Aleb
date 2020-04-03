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
            string bind = Protocol.Localhost;

            if (args.Length == 2 && args[0] == "--bind") bind = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            server = new TcpListener(Dns.GetHostEntry(bind).AddressList[0], Protocol.Port);
            server.Start();

            Console.WriteLine($"Aleb Server started at {server.LocalEndpoint}");

            while (true) Handshake(server.AcceptTcpClient());
        }
    }
}
