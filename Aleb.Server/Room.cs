using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Room {
        public static List<Room> Rooms { get; private set; } = new List<Room>();

        public static Room Create(string name, User creator) {
            if (!Validation.ValidateRoomName(name)) return null;
            if (Rooms.Any(i => i.Name == name || i.Users.Contains(creator))) return null;

            Room room = new Room(name, creator);
            Rooms.Add(room);

            return room;
        }

        public static void Destroy(Room room) => Rooms.Remove(room);

        public class Person {
            public readonly User User;
            public bool Ready = false;

            public Person(User user) => User = user;
        }

        public List<Person> People { get; private set; } = new List<Person>();
        public List<User> Users => People.Select(i => i.User).ToList();

        public int Count => People.Count;

        public bool Join(User joining) {
            if (Count >= 4) return false;
            if (Users.Contains(joining)) return false;

            People.Add(new Person(joining));
            joining.State = UserState.InRoom;
            return true;
        }

        public bool Leave(User leaving) {
            if (!Users.Contains(leaving)) return false;

            if (Game != null)
                DestroyGame(Users.IndexOf(leaving) >> 1);

            People.Remove(People.First(i => i.User == leaving));
            leaving.State = UserState.Idle;

            if (Count == 0) Destroy(this);

            return true;
        }

        public bool SetReady(User user, bool state) {
            Person person = People.FirstOrDefault(i => i.User == user);

            if (person != null) {
                person.Ready = state;
                return true;
            }
            
            return false;
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

        public readonly string Name;

        Room(string name, User owner) {
            Name = name;
            Join(owner);
        }

        public override string ToString() => $"{Name},{Count}{string.Join("", People.Select(i => ',' + i.User.Name))}";
    }
}
