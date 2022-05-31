using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Server {
    class User {
        static List<User> Pool = Persistent.ReadUserPool();

        public static User Connect(string name, string password, AlebClient client) {
            if (!Validation.ValidateUsername(name)) return null;
            if (!Validation.ValidatePassword(password)) return null;

            User user = Pool.FirstOrDefault(i => i?.Name == name);

            if (user == null) {
                user = new User(name, password);
                Pool.Add(user);

                Persistent.SaveUserPool(Pool);

            } else if (user.Password != password) user = null;
            
            if (user != null) user.Client = client;

            return user;
        }

        void Broadcast(IEnumerable<User> users, string command, params dynamic[] args) {
            Message msg = new Message(command, args);

            foreach (User user in users.Where(i => i?.Client?.Connected == true && i != this)) {
                user.Client.Send(msg);
                user.Client.Flush();
            }
        }

        void BroadcastIdle(string command, params dynamic[] args)
            => Broadcast(Pool.Where(i => i.State == UserState.Idle), command, args);

        public UserState State = UserState.Idle;

        public readonly string Name, Password;

        AlebClient _client;
        public AlebClient Client {
            get => _client;
            private set {
                if (_client == value) return;

                _client = value;

                if (_client != null) {
                    _client.MessageReceived += (sender, msg) => Task.Run(() => Received(sender, msg));
                    _client.Disconnected += Disconnect;

                    _client.Name = Name;
                }
            }
        }

        void Received(AlebClient sender, Message msg) {
            if (msg.Command == "UserStats") {
                User user = Pool.FirstOrDefault(i => i.Name == msg.Args[0]);

                if (user != null) {
                    Client.Send(new Message("UserStatsSuccess", new List<string>() {
                        user.Name,
                        user.PointsScored.ToString(),
                        user.GamesPlayed.ToString(),
                        user.GamesWon.ToString(),
                        user.GamesLost.ToString(),
                        user.Bidded.ToString(),
                        user.BidSuccesses.ToString(),
                        user.BidFailures.ToString(),
                        user.Calls20.ToString(),
                        user.Calls50.ToString(),
                        user.Calls100.ToString(),
                        user.Calls150.ToString(),
                        user.Calls200.ToString(),
                        user.SixRow.ToString(),
                        user.SevenRow.ToString(),
                        user.Belotes.ToString(),
                        user.MaxPointsRound.ToString(),
                        user.MaxPointsMatch.ToString()
                    }.ToStr()));
                    
                } else Client.Send(new Message("UserStatsFailure"));

            } else if (State == UserState.Idle) {
                if (msg.Command == "GetRoomList") {
                    Client.Send("RoomList", Room.Rooms.Select(i => i.ToString()).ToArray());
                    
                } else if (msg.Command == "CreateRoom") {
                    Room room = (msg.Args.Length == 5)
                        ? Room.Create(msg.Args[0], msg.Args[1].ToEnum<GameType>(), Convert.ToInt32(msg.Args[2]), Convert.ToBoolean(msg.Args[3]), msg.Args[4], this)
                        : null;

                    if (room != null) {
                        Client.Send("RoomCreated", room.ToString());
                        Client.Send("SpectatorCount", room.Spectators.Count);

                        BroadcastIdle("RoomAdded", room.ToString());
                        
                    } else Client.Send("RoomFailed");
                
                } else if (msg.Command == "JoinRoom") {
                    Room room = (msg.Args.Length == 2)? Room.Rooms.FirstOrDefault(i => i.Name == msg.Args[0]) : null;

                    if (room?.Join(this, msg.Args[1]) == true) {
                        Client.Send("RoomJoined", room.ToString(), room.People.ToStr(i => i.Ready.ToString()));
                        Client.Send("SpectatorCount", room.Spectators.Count);

                        BroadcastIdle("RoomUpdated", room.ToString());
                        Broadcast(room.Everyone, "UserJoined", Name);

                    } else Client.Send("RoomJoinFailed");

                } else if (msg.Command == "SpectateRoom") {
                    Room room = (msg.Args.Length == 2)? Room.Rooms.FirstOrDefault(i => i.Name == msg.Args[0]) : null;

                    if (room?.Spectate(this, msg.Args[1]) == true) {
                        Client.Send("SpectateSuccess", room.ToString(), room.People.ToStr(i => i.Ready.ToString()), room.Game != null);

                        Client.Send("SpectatorCount", room.Spectators.Count);
                        Broadcast(room.Everyone, "SpectatorCount", room.Spectators.Count);

                    } else Client.Send("SpectateFailed");
                }

            } else if (State == UserState.Spectating) {
                if (msg.Command == "SpectatorLeave") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Spectators.Contains(this));

                    if (room?.SpectatorLeave(this) == true && Room.Rooms.Contains(room)) {
                        Client.Send("SpectatorCount", 0);
                        Broadcast(room.Everyone, "SpectatorCount", room.Spectators.Count);
                    }
                    
                } else if (msg.Command == "Reconnecting") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Spectators.Contains(this));
                    
                    room?.Game?.Spectate(this);
                }

            } else if (State == UserState.InRoom) {
                if (msg.Command == "LeaveRoom") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));
                    
                    if (room?.Leave(this) == true) {
                        if (Room.Rooms.Contains(room)) {
                            BroadcastIdle("RoomUpdated", room.ToString());
                            Broadcast(room.Everyone, "UserLeft", Name);
                            
                            Client.Send("SpectatorCount", 0);

                        } else BroadcastIdle("RoomDestroyed", room.Name);
                    }

                } else if (msg.Command == "SetReady") {
                    Room room = (msg.Args.Length == 1)? Room.Rooms.FirstOrDefault(i => i.Users.Contains(this)) : null;
                    bool value = Convert.ToBoolean(msg.Args[0]);

                    if (room?.SetReady(this, value) == true) {
                        Client.Send("UserReady", Name, value);
                        Broadcast(room.Everyone, "UserReady", Name, value);
                    }

                } else if (msg.Command == "SwitchUsers") {
                    Room room = (msg.Args.Length == 2)? Room.Rooms.FirstOrDefault(i => i.Users.Contains(this)) : null;

                    if (room != null && room.Users[0] == this) {
                        User[] switching = Enumerable.Range(0, 2).Select(i => room.Users.FirstOrDefault(j => j.Name == msg.Args[i])).ToArray();

                        if (room.Switch(switching)) {
                            Client.Send("UsersSwitched", switching[0].Name, switching[1].Name);
                            Broadcast(room.Everyone, "UsersSwitched", switching[0].Name, switching[1].Name);
                        }
                    }
                
                } else if (msg.Command == "KickUser") {
                    Room room = (msg.Args.Length == 1)? Room.Rooms.FirstOrDefault(i => i.Users.Contains(this)) : null;

                    if (room != null && room.Users[0] == this) {
                        User kicking = room.Users.FirstOrDefault(i => i.Name == msg.Args[0]);

                        if (room?.Leave(kicking) == true) {
                            kicking.Client.Send("Kicked");
                            kicking.Client.Send("SpectatorCount", room.Spectators.Count);
                            kicking.Client.Flush();

                            BroadcastIdle("RoomUpdated", room.ToString());
                            kicking.Broadcast(room.Everyone, "UserLeft", kicking.Name);
                        }
                    }

                } else if (msg.Command == "StartGame") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));

                    if (room?.Start(this) == true)
                        room.Game.Flush();
                }

            } else if (State == UserState.InGame) {
                if (msg.Command == "Bid") {
                    if (msg.Args.Length == 1)
                        Game.Bid(Player, msg.Args[0].ToEnum<Suit>());
                
                } else if (msg.Command == "TalonBid") {
                    if (msg.Args.Length == 1)
                        Game.TalonBid(Player, Convert.ToInt32(msg.Args[0]));

                } else if (msg.Command == "Declare") {
                    if (msg.Args.Length == 1) {
                        List<int> indexes = msg.Args[0] != "null"? msg.Args[0].ToIntList() : null;

                        Player player = Player; // workaround for belote, game finishes during declare() call

                        bool? result = Game.Declare(player, indexes);
                        if (result.HasValue) player.YouDeclared(result.Value);
                    }

                } else if (msg.Command == "PlayCard") {
                    if (msg.Args.Length == 1)
                        Player.YouPlayed(Game.PlayCard(Player, Convert.ToInt32(msg.Args[0])));
                
                } else if (msg.Command == "Bela")
                    Game.Bela(Player, Convert.ToBoolean(msg.Args[0]));

                else if (msg.Command == "Reconnecting")
                    Game.Reconnect(Player);

                if (Game != null) Game.Flush();
                else Room.Rooms.FirstOrDefault(i => i.Users.Contains(this))
                    .Everyone.ToList().ForEach(i => i?.Client?.Flush());
            }

            Client.Flush();
        }

        void Disconnect(AlebClient sender) {
            if (sender != Client) return;

            Console.WriteLine($"{Client.Address} disconnected");

            Client = null;

            if (State == UserState.InRoom) {
                Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));
                    
                if (room?.Leave(this) == true) {
                    if (Room.Rooms.Contains(room)) {
                        BroadcastIdle("RoomUpdated", room.ToString());
                        Broadcast(room.Everyone, "UserLeft", Name);

                    } else BroadcastIdle("RoomDestroyed", room.Name);
                }
                
            } else if (State == UserState.Spectating) {
                Room room = Room.Rooms.FirstOrDefault(i => i.Spectators.Contains(this));

                if (room?.SpectatorLeave(this) == true && Room.Rooms.Contains(room)) {
                    Broadcast(room.Everyone, "SpectatorCount", room.Spectators.Count);
                }
            }
        }

        User(string name, string password) {
            Name = name;
            Password = password;
        }

        Player Player;
        Game Game;

        public Player ToPlayer(Game game) {
            Game = game;
            return Player = new Player(this);
        }

        public void CompletedGame() {
            Player = null;
            Game = null;
        }

        long _PointsScored;
        public long PointsScored {
            get => _PointsScored;
            set {
                _PointsScored = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _GamesPlayed = 0;
        public int GamesPlayed {
            get => _GamesPlayed;
            set {
                _GamesPlayed = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _GamesWon = 0;
        public int GamesWon {
            get => _GamesWon;
            set {
                _GamesWon = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _GamesLost = 0;
        public int GamesLost {
            get => _GamesLost;
            set {
                _GamesLost = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Bidded = 0;
        public int Bidded {
            get => _Bidded;
            set {
                _Bidded = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _BidSuccesses = 0;
        public int BidSuccesses {
            get => _BidSuccesses;
            set {
                _BidSuccesses = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _BidFailures = 0;
        public int BidFailures {
            get => _BidFailures;
            set {
                _BidFailures = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Calls20 = 0;
        public int Calls20 {
            get => _Calls20;
            set {
                _Calls20 = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Calls50 = 0;
        public int Calls50 {
            get => _Calls50;
            set {
                _Calls50 = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Calls100 = 0;
        public int Calls100 {
            get => _Calls100;
            set {
                _Calls100 = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Calls150 = 0;
        public int Calls150 {
            get => _Calls150;
            set {
                _Calls150 = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Calls200 = 0;
        public int Calls200 {
            get => _Calls200;
            set {
                _Calls200 = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _SixRow = 0;
        public int SixRow {
            get => _SixRow;
            set {
                _SixRow = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _SevenRow = 0;
        public int SevenRow {
            get => _SevenRow;
            set {
                _SevenRow = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _Belotes = 0;
        public int Belotes {
            get => _Belotes;
            set {
                _Belotes = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _MaxPointsRound = 0;
        public int MaxPointsRound {
            get => _MaxPointsRound;
            set {
                _MaxPointsRound = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        int _MaxPointsMatch = 0;
        public int MaxPointsMatch {
            get => _MaxPointsMatch;
            set {
                _MaxPointsMatch = value;
                Persistent.SaveUserPool(Pool);
            }
        }

        public static User FromBinary(BinaryReader reader) {
            string name = reader.ReadString();
            string password = reader.ReadString();

            if (!Validation.ValidateUsername(name)) return null;
            if (!Validation.ValidatePassword(password)) return null;

            User user = new User(name, password);

            user.PointsScored = reader.ReadInt64();
            user.GamesPlayed = reader.ReadInt32();
            user.GamesWon = reader.ReadInt32();
            user.GamesLost = reader.ReadInt32();
            user.Bidded = reader.ReadInt32();
            user.BidSuccesses = reader.ReadInt32();
            user.BidFailures = reader.ReadInt32();
            user.Calls20 = reader.ReadInt32();
            user.Calls50 = reader.ReadInt32();
            user.Calls100 = reader.ReadInt32();
            user.Calls150 = reader.ReadInt32();
            user.Calls200 = reader.ReadInt32();
            user.SixRow = reader.ReadInt32();
            user.SevenRow = reader.ReadInt32();
            user.Belotes = reader.ReadInt32();
            user.MaxPointsRound = reader.ReadInt32();
            user.MaxPointsMatch = reader.ReadInt32();

            return user;
        }

        public void ToBinary(BinaryWriter writer) {
            writer.Write(Name);
            writer.Write(Password);

            writer.Write(PointsScored);
            writer.Write(GamesPlayed);
            writer.Write(GamesWon);
            writer.Write(GamesLost);
            writer.Write(Bidded);
            writer.Write(BidSuccesses);
            writer.Write(BidFailures);
            writer.Write(Calls20);
            writer.Write(Calls50);
            writer.Write(Calls100);
            writer.Write(Calls150);
            writer.Write(Calls200);
            writer.Write(SixRow);
            writer.Write(SevenRow);
            writer.Write(Belotes);
            writer.Write(MaxPointsRound);
            writer.Write(MaxPointsMatch);
        }
    }
}
