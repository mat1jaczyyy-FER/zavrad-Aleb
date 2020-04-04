using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Aleb.Common;

namespace Aleb.Server {
    class User: IDisposable {
        enum UserState {
            Idle, InRoom, InGame   
        }

        static List<User> Pool = new List<User>();

        public static User Create(string name, AlebClient client) {
            if (!Validation.ValidateUserName(name)) return null;
            if (Pool.Any(i => i?.Name == name)) return null;

            User user = new User(name, client);
            Pool.Add(user);

            return user;
        }

        public static void Destroy(User user) {
            if (!Pool.Contains(user)) return;

            Console.WriteLine($"{user.Client.Address} disconnected");

            user.Dispose();
            Pool.Remove(user);
        }

        public static User GetUser(int id)
            => (0 <= id && id < Pool.Count)? Pool[id] : null;

        UserState State = UserState.Idle;

        public readonly string Name;

        public AlebClient Client { get; private set; }

        public delegate void MessageReceivedEventHandler(User sender, Message msg);
        public event MessageReceivedEventHandler MessageReceived;

        public async void ClientLoop() {
            Message msg;

            try {
                msg = await Client.ReadMessage();

            } catch (IOException) {
                Destroy(this);
                return;
            }

            if (Client?.Connected != true) {
                Destroy(this);
                return;
            }

            if (msg.Command == "GetRoomList")
                Client.Send("RoomList", Room.Rooms.Select(i => i.ToString()).ToArray());

            if (msg.Command == "CreateRoom") {
                Room room = Room.Create(msg.Args[0], this);

                if (room != null) Client.Send("RoomCreated", room.ToString());
                else Client.Send("RoomFailed");
            }

            MessageReceived?.Invoke(this, msg);
            ClientLoop();
        }

        User(string name, AlebClient client) {
            Name = name;
            Client = client;

            ClientLoop();
        }

        public void Dispose() {
            MessageReceived = null;

            Client?.Dispose();
            Client = null;
        }

        public Player ToPlayer() => new Player(this);
    }
}
