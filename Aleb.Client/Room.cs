using Aleb.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aleb.Client {
    public class Room {
        public readonly string Name;
        public readonly int Count;

        public Room(string raw) {
            string[] args = raw.Split(',');
            Name = args[0];
            Count = Convert.ToInt32(args[1]);
        }
    }
}
