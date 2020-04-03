using System;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;

namespace Aleb.CLI {
    class Program {
        static async Task<bool> StartConnecting(string host) {
            AlebClient.ConnectStatus result = await AlebClient.Connect(host);

            if (result == AlebClient.ConnectStatus.Success) {
                Console.WriteLine($"Connected to {AlebClient.EndPoint}");
                return true;

            } else if (result == AlebClient.ConnectStatus.VersionMismatch)
                Console.Error.WriteLine($"Server version mismatching, can't connect!");

            else if (result == AlebClient.ConnectStatus.Failed)
                Console.Error.WriteLine($"Failed to connect!");
            
            AlebClient.Disconnect();
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
            
            Console.WriteLine("TODO Login");
            Console.ReadKey();
        }
    }
}
