using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Program {
        public static Func<Task> Menu = Login;

        public static string Username = "";

        public static bool Disconnecting = false;

        public static string ReadLine() {
            Console.Write($"{Username} > ");
            return Console.ReadLine();
        }

        static async Task Login() {
            Task<UserState> stateTask = Requests.ExpectingUserState();
            Task<List<Room>> roomListTask = Requests.ExpectingRoomList();

            do {
                Console.Write("\nUsername: ");
                Username = Console.ReadLine();

                if (Username == "") {
                    Disconnecting = true;
                    return;
                }
                
                Console.Write("Password: ");
                string password = Security.ReadPassword();

                if (!await Requests.Login(Username, password)) {
                    Console.Error.WriteLine("\nLogin failed!");
                    Username = "";
                }

            } while (Username == "");

            Console.WriteLine("\nLogged in!");

            UserState state = await stateTask;

            if (state == UserState.Idle) //{
                RoomMenus.Rooms = await roomListTask;
                Menu = RoomMenus.RoomList;

            //} else if (state == UserState.InGame) {

            //}
        }

        static async Task Main(string[] args) {
            string host = null;

            if (args.Length == 2 && args[0] == "--host") host = args[1];
            else if (args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            Console.WriteLine("Connecting...");
            ConnectStatus result = await Network.Connect(host);

            if (result == ConnectStatus.Success) {
                Console.WriteLine($"Connected to {Network.Server.Address}");

                while (!Disconnecting)
                    await Menu.Invoke();

                Network.Server.Dispose();
                Console.WriteLine("\nDisconnected.");

            } else if (result == ConnectStatus.VersionMismatch)
                Console.Error.WriteLine($"Server version mismatching, can't connect!");

            else if (result == ConnectStatus.Failed)
                Console.Error.WriteLine($"Failed to connect!");

            Console.ReadKey();
        }
    }
}
