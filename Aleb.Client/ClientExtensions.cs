using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public enum ConnectStatus {
        Success, Failed, VersionMismatch
    }

    public static class ClientExtensions {
        public static async Task<(ConnectStatus, AlebClient)> ConnectToServer(string host) {
            TcpClient client = null;

            try {
                client = new TcpClient();
                await client.ConnectAsync(host?? Protocol.Localhost, Protocol.Port);

            } catch {
                client?.Dispose();
                return (ConnectStatus.Failed, null);
            }

            AlebClient entity = new AlebClient(client);

            Message init = await entity.ReadMessage("Version");
            int serverVersion = init == null? -1 : Convert.ToInt32(init.Args[0]);  // todo generalize?

            if (serverVersion != Protocol.Version) {
                entity.Disconnect();
                return (ConnectStatus.VersionMismatch, null);
            }
            
            return (ConnectStatus.Success, entity);
        }

        public static async Task<bool> Login(this AlebClient entity, string name) {
            entity.Send("Login", name);

            Message response = await entity.ReadMessage("LoginResult");
            bool result = response == null? false : Convert.ToBoolean(response.Args[0]);

            return result;
        }
    }
}
