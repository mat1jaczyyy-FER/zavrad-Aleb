using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Server {
    class User {
        static List<User> Pool = new List<User>();
        static IEnumerable<User> Connected => Pool.Where(i => i.Client != null);

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
                user.Client.SendMessage(msg);
                user.Client.Flush();
            }
        }

        void BroadcastIdle(string command, params dynamic[] args)
            => Broadcast(Pool.Where(i => i.State == UserState.Idle), command, args);

        void Broadcast(string command, params dynamic[] args)
            => Broadcast(Pool, command, args);

        public UserState State = UserState.Idle;

        public readonly string Name, Password;

        AlebClient _client;
        public AlebClient Client {
            get => _client;
            private set {
                if (_client == value) return;

                _client = value;

                if (_client != null) {
                    _client.MessageReceived += Received;
                    _client.Disconnected += Disconnect;
                }
            }
        }

        void Received(AlebClient sender, Message msg) {
            if (State == UserState.Idle) {
                if (msg.Command == "GetRoomList") {
                    Client.Send("RoomList", Room.Rooms.Select(i => i.ToString()).ToArray());
                    
                } else if (msg.Command == "CreateRoom") {
                    Room room = (msg.Args.Length == 1)? Room.Create(msg.Args[0], this) : null;

                    if (room != null) {
                        Client.Send("RoomCreated", room.ToString());

                        BroadcastIdle("RoomAdded", room.ToString());
                        
                    } else Client.Send("RoomFailed");
                
                } else if (msg.Command == "JoinRoom") {
                    Room room = (msg.Args.Length == 1)? Room.Rooms.FirstOrDefault(i => i.Name == msg.Args[0]) : null;

                    if (room?.Join(this) == true) {
                        Client.Send("RoomJoined", room.ToString(), string.Join(',', room.People.Select(i => i.Ready)));

                        BroadcastIdle("RoomUpdated", room.ToString());
                        Broadcast(room.Users, "UserJoined", Name);

                    } else Client.Send("RoomJoinFailed");
                }

            } else if (State == UserState.InRoom) {
                if (msg.Command == "LeaveRoom") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));
                    
                    if (room?.Leave(this) == true) {
                        if (Room.Rooms.Contains(room)) {
                            BroadcastIdle("RoomUpdated", room.ToString());
                            Broadcast(room.Users, "UserLeft", Name);

                        } else BroadcastIdle("RoomDestroyed", room.Name);
                    }

                } else if (msg.Command == "SetReady") {
                    Room room = (msg.Args.Length == 1)? Room.Rooms.FirstOrDefault(i => i.Users.Contains(this)) : null;
                    bool value = Convert.ToBoolean(msg.Args[0]);

                    if (room?.SetReady(this, value) == true) {
                        Client.Send("UserReady", Name, value);
                        Broadcast(room.Users, "UserReady", Name, value);
                    }

                } else if (msg.Command == "StartGame") {
                    Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));

                    room?.Start(this);
                }

            } else if (State == UserState.InGame) {
                if (msg.Command == "Bid") {
                    if (msg.Args.Length == 1)
                        Game.Bid(Player, msg.Args[0].ToEnum<Suit>());

                } else if (msg.Command == "Declare") {
                    if (msg.Args.Length == 1) {
                        List<int> indexes = msg.Args[0] != "null"? msg.Args[0].ToIntList() : null;
                        Client.Send("YouDeclared", Game.Declare(Player, indexes));
                    }

                } else if (msg.Command == "PlayCard") {
                    if (msg.Args.Length == 2)
                        Client.Send("YouPlayed", Game.PlayCard(Player, Convert.ToInt32(msg.Args[0]), Convert.ToBoolean(msg.Args[1])));
                }

                Game.Flush();
            }

            Client.Flush();
        }

        void Disconnect(AlebClient sender) {
            Console.WriteLine($"{Client.Address} disconnected");

            Client = null;

            if (State == UserState.InRoom) {
                Room room = Room.Rooms.FirstOrDefault(i => i.Users.Contains(this));
                    
                if (room?.Leave(this) == true) {
                    if (Room.Rooms.Contains(room)) {
                        BroadcastIdle("RoomUpdated", room.ToString());
                        Broadcast(room.Users, "UserLeft", Name);

                    } else BroadcastIdle("RoomDestroyed", room.Name);
                }
            }
        }

        User(string name, string password) {
            Name = name;
            Password = password;
        }

        Player Player;
        Game Game;

        public Player ToPlayer(Game game) { //todo clear these!!
            Game = game;
            return Player = new Player(this);
        }
    }
}
