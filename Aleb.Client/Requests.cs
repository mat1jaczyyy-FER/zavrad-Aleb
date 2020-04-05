using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public static class Requests {
        public static async Task<bool> Login(string name, string password) {
            if (!Validation.ValidateUsername(name)) return false;
            if (!Validation.ValidatePassword(password)) return false;

            return Convert.ToBoolean((await Network.Ask(new Message("Login", name, password), "LoginResult")).Args[0]);
        }

        public static async Task<UserState> ExpectingUserState()                       // generalize for all enums?
            => Enum.Parse<UserState>((await Network.Register("UserState")).Args[0]);
        
        public static async Task<List<Room>> ExpectingRoomList()
            => (await Network.Register("RoomList")).Args.Select(i => new Room(i)).ToList();

        public static async Task<Room> CreateRoom(string name) {
            if (!Validation.ValidateRoomName(name)) return null;

            Message response = await Network.Ask(new Message("CreateRoom", name), "RoomCreated", "RoomFailed");
            return response.Command == "RoomCreated"? new Room(response.Args[0]) : null;
        }
    }
}
