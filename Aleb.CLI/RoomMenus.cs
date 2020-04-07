using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aleb.Client;

namespace Aleb.CLI {
    static class RoomMenus {
        public static Room Room;

        public static async Task InRoom() {
            Console.WriteLine($"\nIn Room: {Room.Name}");

            bool success;

            do {
                success = true;

                Console.WriteLine("\n(T)oggle Ready / (R)efresh / (L)eave Room");

                string action = Program.ReadLine().Trim().ToUpper();
            
                if (action == "R") {}
                else if (action == "L") {

                } else {
                    Console.Error.WriteLine("Invalid input!");
                    success = false;
                }
            } while (!success);
        }
    }
}
