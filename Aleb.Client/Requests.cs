using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static async Task<Room> CreateRoom(string name, GameType type, int goal, bool showpts, string password) {
            name = name?? "";
            password = password?? "";

            if (!Validation.ValidateRoomName(name)) return null;
            if (!Validation.ValidateRoomPassword(password)) return null;

            Message response = await Network.Ask(new Message("CreateRoom", name, type, goal, showpts, password), "RoomCreated", "RoomFailed");
            return response.Command == "RoomCreated"? new Room(response.Args[0]) : null;
        }

        public static async Task<Room> JoinRoom(string name, string password) {
            name = name?? "";
            password = password?? "";

            if (!Validation.ValidateRoomName(name)) return null;
            if (!Validation.ValidateRoomPassword(password)) return null;

            Message response = await Network.Ask(new Message("JoinRoom", name, password), "RoomJoined", "RoomJoinFailed");

            if (response.Command != "RoomJoined") return null;

            Room ret = new Room(response.Args[0]);
            List<bool> ready = response.Args[1].ToList(i => Convert.ToBoolean(i));

            for (int i = 0; i < ready.Count; i++)
                ret.Users[i].Ready = ready[i];

            return ret;
        }

        public static async Task<Tuple<bool, Room>> SpectateRoom(string name, string password) {
            name = name ?? "";
            password = password ?? "";

            if (!Validation.ValidateRoomName(name)) return null;
            if (!Validation.ValidateRoomPassword(password)) return null;

            Message response = await Network.Ask(new Message("SpectateRoom", name, password), "SpectateSuccess", "SpectateFailed");

            if (response.Command != "SpectateSuccess") return null;

            Room ret = new Room(response.Args[0]);
            List<bool> ready = response.Args[1].ToList(i => Convert.ToBoolean(i));
            bool ingame = Convert.ToBoolean(response.Args[2]);

            for (int i = 0; i < ready.Count; i++)
                ret.Users[i].Ready = ready[i];

            return new Tuple<bool, Room>(ingame, ret);
        }

        public static void SpectatorLeave()
            => Network.Send(new Message("SpectatorLeave"));

        public static void LeaveRoom()
            => Network.Send(new Message("LeaveRoom"));

        public static void SetReady(bool state)
            => Network.Send(new Message("SetReady", state));

        public static void SwitchUsers(string user1, string user2)
            => Network.Send(new Message("SwitchUsers", user1, user2));

        public static void KickUser(string user)
            => Network.Send(new Message("KickUser", user));

        public static void StartGame()
            => Network.Send(new Message("StartGame"));

        public static void Reconnecting()
            => Network.Send(new Message("Reconnecting"));

        public static void Bid(Suit? suit)
            => Network.Send(new Message("Bid", suit.ToString()));

        public static void TalonBid(int index)
            => Network.Send(new Message("TalonBid", index));

        public static void Declare(List<int> indexes)
            => Network.Send(new Message("Declare", indexes != null? indexes.ToStr() : "null"));

        public static void PlayCard(int index)
            => Network.Send(new Message("PlayCard", index));

        public static void Bela(bool bela)
            => Network.Send(new Message("Bela", bela));

        public static async Task<UserStats> UserStats(string name) {
            name = name ?? "";
            if (!Validation.ValidateUsername(name)) return null;

            Message response = await Network.Ask(new Message("UserStats", name), "UserStatsSuccess", "UserStatsFail");

            if (response.Command != "UserStatsSuccess") return null;

            return new UserStats(response.Args[0]);
        }
    }
}
