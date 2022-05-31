using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Server {
    class User {
        static List<User> Pool = new List<User>();

        public static User Connect(string name, string password, AlebClient client) {
            if (!Validation.ValidateUsername(name)) return null;
            if (!Validation.ValidatePassword(password)) return null;

            User user = Pool.FirstOrDefault(i => i?.Name == name);

            if (user == null) {
                user = new User(name, password);
                Pool.Add(user);

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
            if (State == UserState.Idle) {
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
    }
}
