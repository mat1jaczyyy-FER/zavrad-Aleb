using System;

using Aleb.Common;

namespace Aleb.Client {
    public class User {
        public readonly string Name;
        public bool Ready;

        public UserState State;

        public User(string raw) => Name = raw;

        public override bool Equals(object obj) {
            if (!(obj is User)) return false;
            return this == (User)obj;
        }

        public static bool operator ==(User a, User b) {
            if (a is null || b is null) return ReferenceEquals(a, b);
            return a.Name == b.Name;
        }
        public static bool operator !=(User a, User b) => !(a == b);

        public override int GetHashCode() => HashCode.Combine(Name);
    }
}
