using System;
using System.Collections.Generic;
using System.IO;
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

        public static User GetUser(int id)
            => (0 <= id && id < Pool.Count)? Pool[id] : null;

        static void Broadcast(Func<User, bool> filter, string command, params dynamic[] args) {
            Message msg = new Message(command, args);

            foreach (User user in Pool.Where(filter)) {
                user.Client.SendMessage(msg);
                user.Client.Flush();
            }
        }

        static void Broadcast(string command, params dynamic[] args) => Broadcast(i => true, command, args);

        public UserState State = UserState.Idle;

        public readonly string Name, Password;

        AlebClient _client;
        public AlebClient Client {
            get => _client;
            private set {
                if (_client == value) return;

                if (_client != null)
                    _client.Dispose();

                _client = value;

                if (_client != null) {
                    _client.MessageReceived += Received;
                    _client.Disconnected += Disconnect;
                    JustConnected();
                }
            }
        }

        void JustConnected() {
            if (State == UserState.Idle) 
                Client.Send("RoomList", Room.Rooms.Select(i => i.ToString()).ToArray());

            else if (State == UserState.InGame) {};

            Client.Send("UserState", State.ToString());
        }

        void Received(AlebClient sender, Message msg) {
            if (State == UserState.Idle) {
                if (msg.Command == "CreateRoom") {
                    Room room = (msg.Args.Length == 1)? Room.Create(msg.Args[0], this) : null;

                    if (room != null) {
                        Client.Send("RoomCreated", room.ToString());
                        Broadcast(i => i.State == UserState.Idle, "RoomAdded", room.ToString());
                        
                    } else Client.Send("RoomFailed");

                    Client.Flush();
                }

            } else if (State == UserState.InRoom) {

            }
        }

        void Disconnect(AlebClient sender) {
            Console.WriteLine($"{Client.Address} disconnected");

            Client = null;
        }

        User(string name, string password) {
            Name = name;
            Password = password;
        }

        public Player ToPlayer() => new Player(this);
    }
}
