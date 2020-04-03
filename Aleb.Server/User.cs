using System.Collections.Generic;

namespace Aleb.Server {
    class User {
        static List<User> Pool = new List<User>();

        public static User Create() {
            User user = new User();
            Pool.Add(user);

            return user;
        }

        public static void Destroy(User user)
            => Pool[Pool.IndexOf(user)] = null;

        public static User GetUser(int id)
            => (0 <= id && id < Pool.Count)? Pool[id] : null;

        public Player ToPlayer() => new Player(this);
    }
}
