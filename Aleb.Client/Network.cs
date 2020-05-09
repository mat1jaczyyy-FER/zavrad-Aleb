using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

            Server.Name = "Server";

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

        public delegate void UsersSwitchedEventHandler(User user1, User user2);
        public static event UsersSwitchedEventHandler UsersSwitched;
        
        public delegate void NothingEventHandler();
        public static event NothingEventHandler Kicked;

        public delegate void GameStartedEventHandler(int dealer, List<int> yourCards);
        public static event GameStartedEventHandler GameStarted;

        public static event NothingEventHandler TrumpNext;

        public delegate void TrumpChosenEventHandler(Suit trump, List<int> yourCards);
        public static event TrumpChosenEventHandler TrumpChosen;
        
        public delegate void YouDeclaredEventHandler(bool result);
        public static event YouDeclaredEventHandler YouDeclared;
        
        public delegate void SimpleIntEventHandler(int value);
        public static event SimpleIntEventHandler PlayerDeclared;

        public delegate void WinningDeclarationEventHandler(int player, int value, List<int> calls, List<int> teammateCalls);
        public static event WinningDeclarationEventHandler WinningDeclaration;

        public static event NothingEventHandler StartPlayingCards;

        public static event SimpleIntEventHandler YouPlayed;
        
        public static event NothingEventHandler AskBela;
        
        public delegate void CardPlayedEventHandler(int card, bool bela);
        public static event CardPlayedEventHandler CardPlayed;

        public delegate void TableCompleteEventHandler(List<int> calls, List<int> played, bool fail);
        public static event TableCompleteEventHandler TableComplete;
        
        public static event SimpleIntEventHandler ContinuePlayingCards;

        public delegate void FinalScoresEventHandler(List<int> final, bool fail);
        public static event FinalScoresEventHandler FinalScores;

        public delegate void TotalScoreEventHandler(List<int> final, bool fail, List<int> total);
        public static event TotalScoreEventHandler TotalScore;

        public delegate void GameFinishedEventHandler(List<int> score, Room room);
        public static event GameFinishedEventHandler GameFinished;

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
            else if (msg.Command == "UsersSwitched") UsersSwitched?.Invoke(new User(msg.Args[0]), new User(msg.Args[1]));
            else if (msg.Command == "Kicked") Kicked?.Invoke();

            else if (msg.Command == "GameStarted") GameStarted?.Invoke(Convert.ToInt32(msg.Args[0]), msg.Args[1].ToIntList());

            else if (msg.Command == "TrumpNext") TrumpNext?.Invoke();
            else if (msg.Command == "TrumpChosen") TrumpChosen?.Invoke(msg.Args[0].ToEnum<Suit>().Value, msg.Args[1].ToIntList());
            
            else if (msg.Command == "YouDeclared") YouDeclared?.Invoke(Convert.ToBoolean(msg.Args[0]));
            else if (msg.Command == "PlayerDeclared") PlayerDeclared?.Invoke(Convert.ToInt32(msg.Args[0]));

            else if (msg.Command == "WinningDeclaration") WinningDeclaration?.Invoke(Convert.ToInt32(msg.Args[0]), Convert.ToInt32(msg.Args[1]), msg.Args[2].ToIntList(), msg.Args[3].ToIntList());
            else if (msg.Command == "StartPlayingCards") StartPlayingCards?.Invoke();

            else if (msg.Command == "YouPlayed") YouPlayed?.Invoke(Convert.ToInt32(msg.Args[0]));
            else if (msg.Command == "AskBela") AskBela?.Invoke();
            else if (msg.Command == "CardPlayed") CardPlayed?.Invoke(Convert.ToInt32(msg.Args[0]), Convert.ToBoolean(msg.Args[1]));

            else if (msg.Command == "TableComplete") TableComplete?.Invoke(msg.Args[0].ToIntList(), msg.Args[1].ToIntList(), Convert.ToBoolean(msg.Args[2]));
            else if (msg.Command == "ContinuePlayingCards") ContinuePlayingCards?.Invoke(Convert.ToInt32(msg.Args[0]));

            else if (msg.Command == "FinalScores") FinalScores?.Invoke(msg.Args[0].ToIntList(), Convert.ToBoolean(msg.Args[1]));
            else if (msg.Command == "TotalScore") TotalScore?.Invoke(msg.Args[0].ToIntList(), Convert.ToBoolean(msg.Args[1]), msg.Args[2].ToIntList());

            else if (msg.Command == "GameFinished") GameFinished?.Invoke(msg.Args[0].ToIntList(), new Room(msg.Args[1]));
        }

        public static Task<Message> Register(params string[] expected) {
            TaskCompletionSource<Message> ret = new TaskCompletionSource<Message>();
            Waiting.Add((expected, ret));
            return ret.Task;
        }

        public static void Send(Message sending) {
            Server.Send(sending);
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
