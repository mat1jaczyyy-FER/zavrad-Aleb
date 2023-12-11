using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Aleb.Common {
    public class AlebClient: IDisposable {
        public static bool LogCommunication = false;

        static Stopwatch time = new Stopwatch();
        public static TimeSpan TimeNow => time.Elapsed;

        void Log(bool received, string raw) {
            if (LogCommunication && Connected && raw != null)
                Console.WriteLine($"{TimeNow} [NETW-{(received? "RECV" : "SEND")}] {Name} ({Address}) > {raw.Trim().Trim('\n')}");
        }

        TcpClient _client;
        TcpClient Client {
            get => _client;
            set {
                Dispose();

                _client = value;

                if (Client != null) {
                    _client.NoDelay = true;

                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());

                    IPEndPoint Remote = (IPEndPoint)Client.Client.RemoteEndPoint;
                    
                    if (Remote == null) Address = "";
                    else Address = $"{Remote.Address.MapToIPv4()}:{Remote.Port}";
                }
            }
        }

        public bool Connected => Client != null;

        StreamReader Reader;
        StreamWriter Writer;
        public string Address { get; private set; } = "";

        public string Name = "<unknown>";

        public delegate void MessageReceivedEventHandler(AlebClient sender, Message msg);
        public event MessageReceivedEventHandler MessageReceived;

        public delegate void DisconnectedEventHandler(AlebClient sender);
        public event DisconnectedEventHandler Disconnected;

        bool Running = false;

        public void Run() {
            if (Running) return;
            Running = true;

            Utilities.FireAndForget(() => {
                while (true) {
                    ResetHeartbeat();

                    string raw;
                    Message msg;

                    try {
                        raw = Reader.ReadLine();
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

        Dictionary<int, Stack<Message>> SendBuffer = new Dictionary<int, Stack<Message>>();
        object locker = new object();

        public void Send(int delay, Message msg) {
            if (!Connected) return;
            
            lock (locker) {
                if (!SendBuffer.ContainsKey(delay))
                    SendBuffer.Add(delay, new Stack<Message>());

                SendBuffer[delay].Push(msg);
            }
        }

        public void Send(int delay, string command, params dynamic[] args)
            => Send(delay, new Message(command, args));
        
        public void Send(Message msg)
            => Send(0, msg);

        public void Send(string command, params dynamic[] args)
            => Send(0, command, args);

        void Flush(Stack<Message> buf) {
            if (!Connected) return;
            
            lock (locker) {
                try {
                    while (buf.TryPop(out Message msg)) { 
                        Log(false, msg.ToString());
                        Writer.Write(msg);
                    }

                    Writer.Flush();

                } catch (IOException) {
                    Client = null;
                    buf.Clear();
                    
                    return;
                }
            }

            ResetHeartbeat();
        }

        public void Flush() {
            if (!Connected) return;

            lock (locker) {
                foreach (var (delay, buf) in SendBuffer) {
                    if (delay <= 0) Flush(buf);
                    else new Courier<Stack<Message>>(delay, buf, Flush);
                }

                SendBuffer.Clear();
            }
        }

        Courier<Message> Heartbeat;
        
        void ResetHeartbeat() {
            Heartbeat?.Cancel();

            Heartbeat = new Courier<Message>(30000, new Message("Heartbeat"), msg => {
                Send(msg);
                Flush();
            });
        }

        public AlebClient(TcpClient client) => Client = client;

        static AlebClient() => time.Start();

        public void Dispose() {
            if (!Connected) return;

            Disconnected?.Invoke(this);

            Client.Dispose();
            _client = null;

            Reader = null;
            Writer = null;
            Address = "";

            MessageReceived = null;
            Disconnected = null;
        }
    }
}
