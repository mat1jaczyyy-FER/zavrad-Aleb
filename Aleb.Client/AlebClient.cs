using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public static class AlebClient {
        static TcpClient client;

        public static bool Connected => client != null;

        public enum ConnectStatus {
            Success, Fail, VersionMismatch, AlreadyConnected
        }

        public static async Task<ConnectStatus> Connect(IPAddress ip = null) {
            if (Connected) return ConnectStatus.AlreadyConnected;

            try {
                client = new TcpClient();
                await client.ConnectAsync(ip?? Protocol.Localhost, 11252);
            } catch {
                Disconnect();
                return ConnectStatus.Fail;
            }

            StreamReader reader = new StreamReader(client.GetStream());
            StreamWriter writer = new StreamWriter(client.GetStream());

            Console.WriteLine($"Connected to {client.Client.RemoteEndPoint}");

            if (Message.Parse("Version", reader.ReadLine(), out Message message)) {
                int serverVersion = Convert.ToInt32(message.Args[0]);

                if (serverVersion != Protocol.Version) return ConnectStatus.VersionMismatch;
            }
            
            return ConnectStatus.Success;
        }

        public static void Disconnect() {
            if (Connected) {
                client.Close();
                client.Dispose();
                client = null;
            }
        }
    }
}
