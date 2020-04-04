using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Aleb.Common;

namespace Aleb.Server {
    class User {
        static List<User> Pool = new List<User>();

        public static User Create(string name, AlebClient client) {
            if (name.Length < 4 || 18 < name.Length) return null;
            if (!name.All(i => char.IsLetterOrDigit(i) || i == '_' || i == '-')) return null;
            if (Pool.Any(i => i.Name == name)) return null;

            User user = new User(name, client);
            Pool.Add(user);

            return user;
        }

        public static void Destroy(User user)
            => Pool[Pool.IndexOf(user)] = null;

        public static User GetUser(int id)
            => (0 <= id && id < Pool.Count)? Pool[id] : null;

        User(string name, AlebClient client) {
            Name = name;
            Client = client;
        }

        public AlebClient Client { get; private set; }

        public readonly string Name;

        public Player ToPlayer() => new Player(this);
    }
}
