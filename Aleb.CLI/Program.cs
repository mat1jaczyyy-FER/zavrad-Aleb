using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Client;
using Aleb.Common;

namespace Aleb.CLI {
    class Program {
        public static Func<Task> Menu = Login;

        public static User User = null;

        public static bool Disconnecting = false;

        public static string ReadLine() {
            Console.Write($"{User.Name} > ");
            return Console.ReadLine();
        }

        static async Task Login() {
            Task<UserState> stateTask = Requests.ExpectingUserState();
            Task<List<Room>> roomListTask = Requests.ExpectingRoomList();

            string name = "";

            do {
                Console.Write("\nUsername: ");
                name = Console.ReadLine();

                if (name == "") {
                    Disconnecting = true;
                    return;
                }
                
                Console.Write("Password: ");
                string password = Security.ReadPassword();

                if (!await Requests.Login(name, password)) {
                    Console.Error.WriteLine("\nLogin failed!");
                    name = "";
                }

            } while (name == "");

            User = new User(name);

            Console.WriteLine("\nLogged in!");

            UserState state = await stateTask;

            if (state == UserState.Idle) {
                IdleMenus.Rooms = await roomListTask;
                Menu = IdleMenus.RoomList;

            } else if (state == UserState.InGame) {

            }
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

                Network.RoomAdded += Events.RoomAdded;

                while (!Disconnecting)
                    await Menu.Invoke();

                Network.Dispose();
                Console.WriteLine("\nDisconnected.");

            } else if (result == ConnectStatus.VersionMismatch)
                Console.Error.WriteLine($"Server version mismatching, can't connect!");

            else if (result == ConnectStatus.Failed)
                Console.Error.WriteLine($"Failed to connect!");

            Console.ReadKey();
        }
    }
}
