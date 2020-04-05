using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Aleb.Common;

namespace Aleb.Server {
    class User {
        public enum UserState {
            Idle, InRoom, InGame   
        }

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

        public static void Disconnect(User user) {
            Console.WriteLine($"{user.Client.Address} disconnected");

            user.Client = null;
        }

        public static User GetUser(int id)
            => (0 <= id && id < Pool.Count)? Pool[id] : null;

        public UserState State = UserState.Idle;

        public readonly string Name, Password;

        AlebClient _client;
        public AlebClient Client {
            get => _client;
            private set {
                if (_client == value) return;

                if (_client != null) {
                    _client.Dispose();
                    MessageReceived = null;
                }

                _client = value;

                if (_client != null) ClientLoop();
            }
        }

        public delegate void MessageReceivedEventHandler(User sender, Message msg);
        public event MessageReceivedEventHandler MessageReceived;

        public async void ClientLoop() {
            Message msg;

            try {
                msg = await Client.ReadMessage(); // todo softlock on reconnect

            } catch (IOException) {
                Disconnect(this);
                return;
            }

            if (Client?.Connected != true || msg == null) {
                Disconnect(this);
                return;
            }

            if (State == UserState.Idle) {
                if (msg.Command == "GetRoomList")
                    Client.Send("RoomList", Room.Rooms.Select(i => i.ToString()).ToArray());

                if (msg.Command == "CreateRoom") {
                    Room room = (msg.Args.Length == 1)? Room.Create(msg.Args[0], this) : null;

                    if (room != null) Client.Send("RoomCreated", room.ToString());
                    else Client.Send("RoomFailed");
                }

            } else if (State == UserState.InRoom) {

            }

            MessageReceived?.Invoke(this, msg);
            ClientLoop();
        }

        User(string name, string password) {
            Name = name;
            Password = password;
        }

        public Player ToPlayer() => new Player(this);
    }
}
