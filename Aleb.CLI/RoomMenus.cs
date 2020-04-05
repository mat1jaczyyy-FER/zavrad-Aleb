using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    static class RoomMenus {
        public static Room Room;

        public static async Task RoomList() {
            List<Room> RoomList = await Network.Server.GetRoomList();

            Console.WriteLine("\nAvailable Rooms:");

            if (!RoomList.Any()) Console.WriteLine("None");

            for (int i = 0; i < RoomList.Count; i++)
                Console.WriteLine($"{i}. {RoomList[i].Display()}");

            bool success;

            do {
                success = true;

                Console.WriteLine("\n(R)efresh / (int) Join Room / (C)reate Room / (D)isconnect");

                string action = Program.ReadLine().Trim().ToUpper();
            
                if (action == "R") {}
                else if (action == "C") Program.Menu = CreateRoom;
                else if (action == "D") Program.Disconnecting = true;
                else if (int.TryParse(action, out int index)) {
                    // todo impl join room

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
            
                if ((Room = await Network.Server.CreateRoom(name)) == null) {
                    Console.Error.WriteLine("Couldn't create a room with this name.");
                    success = false;
                }
            } while (!success);

            Program.Menu = InRoom;
        }

        public static async Task InRoom() {
            await Task.Delay(1000);
        }
    }
}
