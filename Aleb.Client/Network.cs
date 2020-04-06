using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Client {
    public enum ConnectStatus {
        Success, Failed, VersionMismatch
    }

    public static class Network {
        public static AlebClient Server { get; private set; } = null;

        public static async Task<ConnectStatus> Connect(string host) {
            TcpClient tcp = null;

            try {
                tcp = new TcpClient();
                await tcp.ConnectAsync(host?? Protocol.Localhost, Protocol.Port);

            } catch {
                tcp?.Dispose();
                return ConnectStatus.Failed;
            }

            AlebClient.LogCommunication = true;

            Server = new AlebClient(tcp);

            Server.MessageReceived += Received;
            Task<Message> VersionTask = Register("Version");

            Server.Run();

            await VersionTask;

            if (Convert.ToInt32((await VersionTask).Args[0]) != Protocol.Version) {
                Server.Dispose();
                Server = null;

                return ConnectStatus.VersionMismatch;
            }
            
            return ConnectStatus.Success;
        }

        static HashSet<(string[] Expected, TaskCompletionSource<Message> TCS)> Waiting = new HashSet<(string[], TaskCompletionSource<Message>)>();
        
        public delegate void RoomUpdatedEventHandler(Room room);
        public static event RoomUpdatedEventHandler RoomAdded;

        static void Received(AlebClient sender, Message msg) {
            foreach (var i in Waiting.ToHashSet()) {
                if (i.Expected.Contains(msg.Command)) {
                    Task.Run(() => i.TCS.TrySetResult(msg));
                    Waiting.Remove(i);
                }
            }

            if (msg.Command == "RoomAdded") RoomAdded?.Invoke(new Room(msg.Args[0]));
        }

        public static Task<Message> Register(params string[] expected) {
            TaskCompletionSource<Message> ret = new TaskCompletionSource<Message>();
            Waiting.Add((expected, ret));
            return ret.Task;
        }

        public static Task<Message> Ask(Message sending, params string[] expected) {
            Task<Message> VersionTask = Register(expected);

            Server.SendMessage(sending);
            Server.Flush();

            return VersionTask;
        }

        public static void Dispose() {
            Server?.Dispose();
            Server = null;

            RoomAdded = null;
        }
    }
}
