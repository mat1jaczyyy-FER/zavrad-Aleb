using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace Aleb.Common {
    public class AlebClient: IDisposable {
        public static bool LogCommunication = false;

        void Log(bool received, string raw) {
            if (LogCommunication && Connected && raw != null)
                Console.WriteLine($"[NETW-{(received? "RECV" : "SEND")}] {Address} > {raw.Trim(' ', '\n')}");
        }

        TcpClient _client;
        TcpClient Client {
            get => _client;
            set {
                _client = value;

                if (Client == null) {
                    Reader = null;
                    Writer = null;
                    Address = "";

                } else {
                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());

                    Address = ((IPEndPoint)Client?.Client.RemoteEndPoint).Address.MapToIPv4().ToString();
                }
            }
        }

        public bool Connected => Client != null;

        StreamReader Reader;
        StreamWriter Writer;
        public string Address { get; private set; } = "";

        public async Task<Message> ReadMessage(params string[] expected) {
            if (!Connected) return null;

            string raw = await Reader.ReadLineAsync();
            Log(true, raw);

            return Message.Parse(raw, expected);
        }

        public async Task<T> ReadMessage<T>(Func<Message, T> success, T fail, params string[] expected) {
            Message msg = await ReadMessage(expected);
            return msg == null? fail : success.Invoke(msg);
        }

        public void Send(string command, params dynamic[] args)
            => SendMessage(new Message(command, args));

        public void SendMessage(Message msg) {
            if (!Connected) return;

            Log(false, msg.ToString());

            Writer.Write(msg);
            Writer.Flush();
        }

        public AlebClient(TcpClient client) => Client = client;

        public void Dispose() {
            if (!Connected) return;

            Client.Dispose();
            Client = null;
        }
    }
}
