using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Network {
        public static AlebClient Server { get; private set; }

        public static async Task<bool> Connect(string host) {
            Console.WriteLine("Connecting...");

            ConnectStatus result;
            (result, Server) = await ClientExtensions.ConnectToServer(host);

            if (result == ConnectStatus.Success) {
                Console.WriteLine($"Connected to {Server.Address}");
                return true;

            } else if (result == ConnectStatus.VersionMismatch)
                Console.Error.WriteLine($"Server version mismatching, can't connect!");

            else if (result == ConnectStatus.Failed)
                Console.Error.WriteLine($"Failed to connect!");

            return false;
        }
    }
}
