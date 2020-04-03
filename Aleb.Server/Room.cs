using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    class Room {
        class Person {
            public readonly User User;
            public bool Ready = false;

            public Person(User user) => User = user;
        }

        List<Person> People = new List<Person>();
        public List<User> Users => People.Select(i => i.User).ToList();

        public int Count => People.Count;

        public bool Join(User joining) {
            if (Count >= 4) return false;
            if (Users.Contains(joining)) return false;

            People.Add(new Person(joining));
            return true;
        }

        public bool Leave(User leaving) {
            if (!Users.Contains(leaving)) return false;

            if (Game != null)
                DestroyGame(Users.IndexOf(leaving) >> 1);

            People.Remove(People.First(i => i.User == leaving));

            if (Count == 0) Manager.DestroyRoom(this);

            return true;
        }

        public Game Game { get; private set; }

        public bool GameCompleted(int[] score) {
            bool done = false;
            int winner = -1;

            for (int i = 0; i < 2; i++) {
                if (score[i] >= 1001) done = true;
                if (score[i] > score[1 - i]) winner = i;
            }

            if (!done) return false;

            DestroyGame(winner);

            return true;
        }

        void DestroyGame(int winner) {
            // todo send winscreen request

            Game = null;
        }

        public bool Start(User starting) {
            if (Game != null || starting != People[0].User || Count < 4 || People.Skip(1).Any(i => !i.Ready))
                return false;

            Game = new Game(this);
            People.ForEach(i => i.Ready = false);

            return true;
        }

        public Room(User owner) => Join(owner);
    }
}
