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
        static AlebClient Server;

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
            Server.Disconnected += _ => Disconnected?.Invoke();

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
        
        public delegate void DisconnectedEventHandler();
        public static event DisconnectedEventHandler Disconnected;

        static HashSet<(string[] Expected, TaskCompletionSource<Message> TCS)> Waiting = new HashSet<(string[], TaskCompletionSource<Message>)>();
        
        public delegate void RoomUpdatedEventHandler(Room room);
        public static event RoomUpdatedEventHandler RoomAdded, RoomUpdated;

        public delegate void RoomDestroyedEventHandler(string roomName);
        public static event RoomDestroyedEventHandler RoomDestroyed;

        public delegate void UserEventHandler(User user);
        public static event UserEventHandler UserJoined, UserLeft, UserReady;

        public delegate void GameStartedEventHandler(int dealer, List<int> yourCards);
        public static event GameStartedEventHandler GameStarted;

        public delegate void TrumpNextEventHandler(int playing);
        public static event TrumpNextEventHandler TrumpNext;

        public delegate void TrumpChosenEventHandler(int selector, Suit trump, List<int> yourCards);
        public static event TrumpChosenEventHandler TrumpChosen;

        static void Received(AlebClient sender, Message msg) {
            foreach (var i in Waiting.ToHashSet()) {
                if (i.Expected.Contains(msg.Command)) {
                    Task.Run(() => i.TCS.TrySetResult(msg));
                    Waiting.Remove(i);
                }
            }

            if (msg.Command == "RoomAdded") RoomAdded?.Invoke(new Room(msg.Args[0]));
            else if (msg.Command == "RoomUpdated") RoomUpdated?.Invoke(new Room(msg.Args[0]));
            else if (msg.Command == "RoomDestroyed") RoomDestroyed?.Invoke(msg.Args[0]);

            else if (msg.Command == "UserJoined") UserJoined?.Invoke(new User(msg.Args[0]));
            else if (msg.Command == "UserLeft") UserLeft?.Invoke(new User(msg.Args[0]));
            else if (msg.Command == "UserReady") UserReady?.Invoke(new User(msg.Args[0]) { Ready = Convert.ToBoolean(msg.Args[1]) });

            else if (msg.Command == "GameStarted") GameStarted?.Invoke(Convert.ToInt32(msg.Args[0]), msg.Args[1].Split(',').Select(i => Convert.ToInt32(i)).ToList());

            else if (msg.Command == "TrumpNext") TrumpNext?.Invoke(Convert.ToInt32(msg.Args[0]));
            else if (msg.Command == "TrumpChosen") TrumpChosen?.Invoke(Convert.ToInt32(msg.Args[0]), msg.Args[1].ToEnum<Suit>().Value, msg.Args[2].Split(',').Select(i => Convert.ToInt32(i)).ToList());
        }

        public static Task<Message> Register(params string[] expected) {
            TaskCompletionSource<Message> ret = new TaskCompletionSource<Message>();
            Waiting.Add((expected, ret));
            return ret.Task;
        }

        public static void Send(Message sending) {
            Server.SendMessage(sending);
            Server.Flush();
        } 

        public static Task<Message> Ask(Message sending, params string[] expected) {
            Task<Message> VersionTask = Register(expected);
            Send(sending);

            return VersionTask;
        }

        public static void Dispose() {
            Server?.Dispose();
            Server = null;

            Disconnected = null;

            RoomAdded = null;
            RoomUpdated = null;
            RoomDestroyed = null;

            UserJoined = null;
            UserLeft = null;
            UserReady = null;

            GameStarted = null;
        }
    }
}
