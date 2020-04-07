using System;
using System.Collections.Generic;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public class Room {
        public readonly string Name;
        public int Count;

        public User[] Users = new User[4];

        public Room(string raw) {
            string[] args = raw.Split(',');
            Name = args[0];
            Count = Convert.ToInt32(args[1]);

            for (int i = 2; i < args.Length; i++)
                Users[i - 2] = new User(args[i]);
        }

        public override bool Equals(object obj) {
            if (!(obj is Room)) return false;
            return this == (Room)obj;
        }

        public static bool operator ==(Room a, Room b) {
            if (a is null || b is null) return ReferenceEquals(a, b);
            return a.Name == b.Name;
        }
        public static bool operator !=(Room a, Room b) => !(a == b);

        public override int GetHashCode() => HashCode.Combine(Name);
    }
}
