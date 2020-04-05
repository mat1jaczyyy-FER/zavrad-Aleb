using System;
using System.Collections.Generic;
using System.Text;

namespace Aleb.CLI {
    static class Security {
        public static string ReadPassword() {
            StringBuilder pw = new StringBuilder();

            while (true) {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) {
                    Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace) {
                    if (pw.Length > 0) pw.Length--;
                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                    pw.Append(key.KeyChar);
            }

            return pw.ToString();
        }
    }
}
