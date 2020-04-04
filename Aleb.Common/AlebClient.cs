using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace Aleb.Common {
    public class AlebClient: IDisposable {
        TcpClient _client;
        TcpClient Client {
            get => _client;
            set {
                _client = value;

                if (Client == null) {
                    Reader = null;
                    Writer = null;

                } else {
                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());
                }
            }
        }

        public EndPoint EndPoint => Client?.Client.RemoteEndPoint;
        public bool Connected => Client != null;

        StreamReader Reader;
        StreamWriter Writer;

        public async Task<Message> ReadMessage(params string[] expected) {
            if (!Connected) return null;

            return Message.Parse(await Reader.ReadLineAsync(), expected);
        }

        public void Send(string command, params dynamic[] args)
            => SendMessage(new Message(command, args));

        public void SendMessage(Message msg) {
            if (!Connected) return;

            Writer.Write(msg);
            Writer.Flush();
        }

        public AlebClient(TcpClient client) => Client = client;

        public void Disconnect() {
            if (!Connected) return;

            Client.Dispose();
            Client = null;
        }

        public void Dispose() => Disconnect();
    }
}
