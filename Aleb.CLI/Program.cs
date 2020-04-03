using System;
using System.Threading.Tasks;

using Aleb.Client;

namespace Aleb.CLI {
    class Program {
        static async Task<bool> StartConnecting() {
            AlebClient.ConnectStatus result = await AlebClient.Connect();

            if (result == AlebClient.ConnectStatus.Success) return true;

            switch (await AlebClient.Connect()) {
                case AlebClient.ConnectStatus.VersionMismatch:
                    Console.Error.WriteLine($"Server version mismatching, can't connect!");
                    break;

                case AlebClient.ConnectStatus.Fail:
                    Console.Error.WriteLine($"Failed to connect!");
                    break;
            }
            
            AlebClient.Disconnect();
            return false;
        }

        static async Task Main(string[] args) {
            Console.WriteLine("Connecting...");

            if (!await StartConnecting()) {
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("TODO Login");
            Console.ReadKey();
        }
    }
}
