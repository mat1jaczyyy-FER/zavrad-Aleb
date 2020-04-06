using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

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
                    Disconnected?.Invoke(this);
                    Disconnected = null;

                    Reader = null;
                    Writer = null;
                    Address = "";
                    MessageReceived = null;

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

        public delegate void MessageReceivedEventHandler(AlebClient sender, Message msg);
        public event MessageReceivedEventHandler MessageReceived;

        public delegate void DisconnectedEventHandler(AlebClient sender);
        public event DisconnectedEventHandler Disconnected;

        bool Running = false;

        public void Run() {
            if (Running) return;
            Running = true;

            Task.Run(() => {
                while (true) {
                    string raw;
                    Message msg;

                    try {
                        raw = Reader.ReadLine(); // todo softlock on reconnect?
                        msg = Message.Parse(raw);

                    } catch (IOException) {
                        Client = null;
                        return;
                    }

                    if (msg == null) {
                        Client = null;
                        return;
                    }

                    Log(true, raw);
                    
                    MessageReceived?.Invoke(this, msg);
                }
            });
        }

        Stack<Message> SendBuffer = new Stack<Message>();

        public void SendMessage(Message msg) => SendBuffer.Push(msg);

        public void Send(string command, params dynamic[] args)
            => SendMessage(new Message(command, args));

        public void Flush() {
            if (!Connected) return;

            while (SendBuffer.TryPop(out Message msg)) { 
                Log(false, msg.ToString());
                Writer.Write(msg);
            }

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
