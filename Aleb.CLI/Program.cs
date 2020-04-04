using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Program {
        static async Task Main(string[] args) {
            string host = null;

            if (args.Length == 2 && args[0] == "--host") host = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            Console.WriteLine("Connecting...");

            if (!await Network.Connect(host)) {
                Console.ReadKey();
                return;
            }
            
            string name = "";

            do {
                Console.Write("\nUsername: ");

                if (!await Network.Server.Login(name = Console.ReadLine())) {
                    Console.Error.WriteLine("Login failed!");
                    name = "";
                }

            } while (name == "");

            List<Room> RoomList = await Network.Server.GetRoomList();

            Console.WriteLine("\nAvailable Rooms:");

            if (!RoomList.Any()) Console.WriteLine("None");

            for (int i = 0; i < RoomList.Count; i++)
                Console.WriteLine($"{i}. {RoomList[i].Display()}");

            Console.WriteLine("\n(R)efresh / (int) Join Room / (D)isconnect");

            Console.ReadKey();
        }
    }
}
