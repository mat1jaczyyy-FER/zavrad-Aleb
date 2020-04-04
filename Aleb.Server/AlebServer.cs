using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Aleb.Common;

namespace Aleb.Server {
    class AlebServer {
        static TcpListener server;

        static async void Handshake(TcpClient client) {
            AlebClient entity = new AlebClient(client);

            Console.WriteLine($"Connection from {entity.Address}");

            entity.Send("Version", Protocol.Version);
            
            User user = null;

            try {
                bool success = false;

                do {
                    Message response = await entity.ReadMessage("Login");
                    string name = response == null? "" : response.Args[0];
                    user = User.Create(name, entity);

                    entity.Send("LoginResult", success = (user != null));

                } while (!success);

            } catch {
                Console.WriteLine($"{entity.Address} disconnected without logging in");
                entity.Dispose();
                return;
            }

            Console.WriteLine($"{user.Name} logged in from {entity.Address}");
        }

        static void Main(string[] args) {
            AlebClient.LogCommunication = true;

            string bind = Protocol.Localhost;

            if (args.Length == 2 && args[0] == "--bind") bind = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            server = new TcpListener(
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

            while (true) Handshake(server.AcceptTcpClient());
        }
    }
}
