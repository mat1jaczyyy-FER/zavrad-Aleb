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

        public static EndPoint EndPoint => client?.Client.RemoteEndPoint;

        public static bool Connected => client != null;

        public enum ConnectStatus {
            Success, Failed, VersionMismatch, AlreadyConnected
        }

        public static async Task<ConnectStatus> Connect(string host) {
            if (Connected) return ConnectStatus.AlreadyConnected;

            try {
                client = new TcpClient();
                await client.ConnectAsync(host?? Protocol.Localhost, 11252);
            } catch {
                Disconnect();
                return ConnectStatus.Failed;
            }

            StreamReader reader = new StreamReader(client.GetStream());

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
