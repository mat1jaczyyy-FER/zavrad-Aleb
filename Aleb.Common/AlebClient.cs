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
            if (LogCommunication)
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

                } else {
                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());
                }
            }
        }

        public string Address {
            get {
                IPAddress IP = ((IPEndPoint)Client?.Client.RemoteEndPoint).Address;
                IPHostEntry entry = Dns.GetHostEntry(IP);

                return entry?.HostName?? IP.MapToIPv4().ToString();
            }
        }

        public bool Connected => Client != null;

        StreamReader Reader;
        StreamWriter Writer;

        public async Task<Message> ReadMessage(params string[] expected) {
            if (!Connected) return null;

            string raw = await Reader.ReadLineAsync();
            Log(true, raw);

            return Message.Parse(raw, expected);
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
