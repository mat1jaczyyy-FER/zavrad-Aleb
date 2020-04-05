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
            do {
                Console.Write("\nUsername: ");
                Username = Console.ReadLine();

                if (Username == "") {
                    Disconnecting = true;
                    return;
                }
                
                Console.Write("Password: ");
                string password = Security.ReadPassword();

                if (!await Network.Server.Login(Username, password)) {
                    Console.Error.WriteLine("\nLogin failed!");
                    Username = "";
                }

            } while (Username == "");

            Console.WriteLine("\nLogged in!");
            Menu = RoomMenus.RoomList;
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

            Network.Server.Dispose();
            Console.WriteLine("\nDisconnected.");

            Console.ReadKey();
        }
    }
}
