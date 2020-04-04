using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aleb.Common {
    public class Message {
        public readonly bool Valid;
        public readonly string Command;
        public readonly string[] Args;

        public static Message Parse(string raw, params string[] expected) {
            Message ret = new Message(raw);

            return ret.Valid && (!expected.Any() || expected.Contains(ret.Command))
                ? ret
                : null;
        }

        protected Message(string raw) {
            IEnumerable<string> args = raw?.Split(' ').Select(i => i.Trim(' ', '\n'));
            if (args?.Any() != true) return;

            Command = args.First();
            Args = args.Skip(1).ToArray();
            Valid = true;
        }

        public Message(string command, params dynamic[] args) {
            Command = command;
            Args = args.Select(i => (string)i.ToString()).ToArray();
            Valid = true;
        }

        public override string ToString()
            => $"{Command}{string.Join("", Args.Select(i => ' ' + i).ToArray())}\n";
    }
}
