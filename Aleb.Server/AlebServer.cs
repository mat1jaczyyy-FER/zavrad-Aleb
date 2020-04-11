using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Aleb.Common;

namespace Aleb.Server {
    static class AlebServer {
        static void Login(AlebClient sender, Message msg) {
            if (msg.Command != "Login") return;

            User user = User.Connect(msg.Args[0], msg.Args[1], sender);

            sender.Send("LoginResult", user?.State.ToString()?? "null");

            if (user != null) {
                sender.MessageReceived -= Login;
                sender.Disconnected -= Disconnect;

                Console.WriteLine($"{user.Name} logged in from {sender.Address}");
            }
            
            sender.Flush();
        }

        static void Disconnect(AlebClient sender)
            => Console.WriteLine($"{sender.Address} disconnected without logging in");

        static void Main(string[] args) {
            AlebClient.LogCommunication = true;

            string bind = Protocol.Localhost;

            if (args.Length == 2 && args[0] == "--bind") bind = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            TcpListener server = new TcpListener(
                Dns.GetHostEntry(bind).AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork),
                Protocol.Port
            );

            try {
                server.Start();

            } catch {
                Console.Error.WriteLine("Failed to start server! Is the port already in use?");
                return;
            }

            Console.WriteLine($"Aleb Server started at {server.LocalEndpoint}");

            while (true) {
                AlebClient client = new AlebClient(server.AcceptTcpClient());

                Console.WriteLine($"Connection from {client.Address}");

                client.MessageReceived += Login;
                client.Disconnected += Disconnect;

                client.Run();

                client.Send("Version", Protocol.Version);
                client.Flush();
            }
        }
    }
}
