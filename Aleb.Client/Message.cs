using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aleb.Client {
    class Message {
        public readonly bool Valid;
        public readonly string Command;
        public readonly string[] Args;

        public static bool Parse(string raw, out Message msg) {
            msg = new Message(raw);
            return msg.Valid;
        }

        public static bool Parse(string expected, string raw, out Message msg)
            => Parse(raw, out msg) && msg.Command == expected;

        protected Message(string raw) {
            IEnumerable<string> args = raw?.Split(' ').Select(i => i.Trim());
            if (args?.Any() == false) return;

            Command = args.First();
            Args = args.Skip(1).ToArray();
            Valid = true;
        }
    }
}
