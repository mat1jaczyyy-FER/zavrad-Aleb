using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Program {
        static Func<Task> Menu = Login;
        static string Username = "";

        static Room Room;

        static bool Disconnecting = false;

        static async Task Login() {
            do {
                Console.Write("\nUsername: ");
                Username = Console.ReadLine();

                if (Username == "") {
                    Disconnecting = true;
                    return;
                }

                if (!await Network.Server.Login(Username)) {
                    Console.Error.WriteLine("Login failed!");
                    Username = "";
                }

            } while (Username == "");

            Console.WriteLine("Logged in!");
            Menu = RoomListMenu;
        }

        static async Task RoomListMenu() {
            List<Room> RoomList = await Network.Server.GetRoomList();

            Console.WriteLine("\nAvailable Rooms:");

            if (!RoomList.Any()) Console.WriteLine("None");

            for (int i = 0; i < RoomList.Count; i++)
                Console.WriteLine($"{i}. {RoomList[i].Display()}");

            bool success;

            do {
                success = true;

                Console.WriteLine("\n(R)efresh / (int) Join Room / (C)reate Room / (D)isconnect");

                string action = Console.ReadLine().Trim().ToUpper();
            
                if (action == "R") {}
                else if (action == "C") Menu = CreateRoomMenu;
                else if (action == "D") Disconnecting = true;
                else if (int.TryParse(action, out int index)) {
                    // impl join room

                } else {
                    Console.Error.WriteLine("Invalid input!");
                    success = false;
                }
            } while (!success);
        }

        static async Task CreateRoomMenu() {
            bool success;

            do {
                success = true;

                Console.WriteLine("\nEnter Room Name, or leave empty to cancel:");
                string name = Console.ReadLine().Trim();

                if (name == "") {
                    Menu = RoomListMenu;
                    return;
                }
            
                if ((Room = await Network.Server.CreateRoom(name)) == null) {
                    Console.Error.WriteLine("Couldn't create a room with this name.");
                    success = false;
                }
            } while (!success);

            Menu = InRoomMenu;
        }

        static async Task InRoomMenu() {
            await Task.Delay(1000);
        }

        static async Task Main(string[] args) {
            string host = null;

            if (args.Length == 2 && args[0] == "--host") host = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            if (await Network.Connect(host))
                while (!Disconnecting)
                    await Menu.Invoke();

            Console.ReadKey();
        }
    }
}
