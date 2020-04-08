using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public class Room {
        public readonly string Name;
        public int Count;

        User[] _users = new User[4];
        public User[] Users { 
            get => _users;
            set {
                for (int i = 0; i < 4; i++)
                    _users[i] = i < value.Length? value[i] : null;
            }
        }

        public Room(string raw) {
            string[] args = raw.Split(',');
            Name = args[0];
            Count = Convert.ToInt32(args[1]);
            Users = args.Skip(2).Select(i => new User(i)).ToArray();
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
