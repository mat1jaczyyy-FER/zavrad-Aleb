using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public static class Requests {
        public static async Task<User> Login(string name, string password) {
            name = name?? "";
            password = password?? "";

            if (!Validation.ValidateUsername(name)) return null;
            if (!Validation.ValidatePassword(password)) return null;

            if (Enum.TryParse((await Network.Ask(new Message("Login", name, password), "LoginResult")).Args[0], false, out UserState state))
                return new User(name) { State = state };

            return null;
        }
        
        public static async Task<List<Room>> GetRoomList()
            => (await Network.Ask(new Message("GetRoomList"), "RoomList"))?.Args.Select(i => new Room(i)).ToList()?? new List<Room>();

        public static async Task<Room> CreateRoom(string name) {
            name = name?? "";

            if (!Validation.ValidateRoomName(name)) return null;

            Message response = await Network.Ask(new Message("CreateRoom", name), "RoomCreated", "RoomFailed");
            return response.Command == "RoomCreated"? new Room(response.Args[0]) : null;
        }

        public static async Task<Room> JoinRoom(string name) {
            name = name?? "";

            if (!Validation.ValidateRoomName(name)) return null;

            Message response = await Network.Ask(new Message("JoinRoom", name), "RoomJoined", "RoomJoinFailed");
            return response.Command == "RoomJoined"? new Room(response.Args[0]) : null;
        }
    }
}
