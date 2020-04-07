using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aleb.Client;

namespace Aleb.CLI {
    static class IdleMenus {
        public static List<Room> Rooms = new List<Room>();

        public static async Task RoomList() {
            Console.WriteLine("\nAvailable Rooms:");

            List<Room> rooms = Rooms.ToList();

            if (!rooms.Any()) Console.WriteLine("None");

            for (int i = 0; i < rooms.Count; i++)
                Console.WriteLine($"{i}. {rooms[i].Display()}");

            bool success;

            do {
                success = true;

                Console.WriteLine("\n(R)efresh / (int) Join Room / (C)reate Room / (D)isconnect");

                string action = Program.ReadLine().Trim().ToUpper();
            
                if (action == "R") {}
                else if (action == "C") Program.Menu = CreateRoom;
                else if (action == "D") Program.Disconnecting = true;
                else if (int.TryParse(action, out int index) && 0 <= index && index < rooms.Count) {
                    if ((RoomMenus.Room = await Requests.JoinRoom(rooms[index].Name)) == null) {
                        Console.Error.WriteLine("Couldn't join that room!");
                        success = false;

                    } else Program.Menu = RoomMenus.InRoom;

                } else {
                    Console.Error.WriteLine("Invalid input!");
                    success = false;
                }
            } while (!success);
        }

        public static async Task CreateRoom() {
            bool success;

            do {
                success = true;

                Console.Write("\nEnter Room Name, or leave empty to cancel: ");
                string name = Console.ReadLine().Trim();

                if (name == "") {
                    Program.Menu = RoomList;
                    return;
                }
            
                if ((RoomMenus.Room = await Requests.CreateRoom(name)) == null) {
                    Console.Error.WriteLine("Couldn't create a room with this name.");
                    success = false;
                }
            } while (!success);

            Program.Menu = RoomMenus.InRoom;
        }
    }
}
