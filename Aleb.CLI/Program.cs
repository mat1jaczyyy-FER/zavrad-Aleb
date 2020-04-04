using System;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Program {
        static AlebClient Server;

        static async Task<bool> StartConnecting(string host) {
            ConnectStatus result;
            (result, Server) = await ClientExtensions.ConnectToServer(host);

            if (result == ConnectStatus.Success) {
                Console.WriteLine($"Connected to {Server.EndPoint}");
                return true;

            } else if (result == ConnectStatus.VersionMismatch)
                Console.Error.WriteLine($"Server version mismatching, can't connect!");

            else if (result == ConnectStatus.Failed)
                Console.Error.WriteLine($"Failed to connect!");

            return false;
        }

        static async Task Main(string[] args) {
            string host = null;

            if (args.Length == 2 && args[0] == "--host") host = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            Console.WriteLine("Connecting...");

            if (!await StartConnecting(host)) {
                Console.ReadKey();
                return;
            }
            
            string name = "";

            do {
                Console.Write("\nUsername: ");

                if (!await Server.Login(name = Console.ReadLine())) {
                    Console.Error.WriteLine("Login failed!");
                    name = "";
                }

            } while (name == "");

            Console.WriteLine("TODO Expect RoomList");

            Console.ReadKey();
        }
    }
}
