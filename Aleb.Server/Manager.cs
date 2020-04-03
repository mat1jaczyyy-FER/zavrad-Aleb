using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    static class Manager {
        public static List<Room> Rooms { get; private set; } = new List<Room>();

        public static Room CreateRoom(User creator) {
            if (Rooms.Any(i => i.Users.Contains(creator))) return null;

            Room room = new Room(creator);
            Rooms.Add(room);

            return room;
        }

        public static void DestroyRoom(Room room) => Rooms.Remove(room);
    }
}
