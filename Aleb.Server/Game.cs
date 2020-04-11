using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    class Game {
        void Flush() {
            foreach (Player player in Players)
                player.Flush();
        }

        void Broadcast(int exclude, string command, params dynamic[] args) {
            foreach (Player player in Players.Where((_, i) => i != exclude))
                player.SendMessage(command, args);
        }

        void Broadcast(string command, params dynamic[] args) => Broadcast(-1, command, args);

        int Modulo(int n, int m) => (n + m) % m;

        enum GameState {
            Bidding,
            Declaring,
            Playing
        }

        Player[] Players = new Player[4];

        GameState State;

        Table Table;

        Player Dealer, Current;

        List<Round> History = new List<Round>();
        int[] Score => Enumerable.Range(0, 2).Select(i => History.Where(x => x.Finalized).Sum(x => x.Played[i])).ToArray();

        Room Room;

        public Game(Room room) {
            Room = room;

            Players[0] = Room.Users[0].ToPlayer();
            Players[1] = Room.Users[2].ToPlayer();
            Players[2] = Room.Users[1].ToPlayer();
            Players[3] = Room.Users[3].ToPlayer();

            for (int i = 0; i < 4; i++) {
                Players[i].Previous = Players[Modulo(i - 1, 4)];
                Players[i].Next = Players[Modulo(i + 1, 4)];
                Players[i].Teammate = Players[Modulo(i + 2, 4)];
                Players[i].Team = i % 2;
            }

            Dealer = Players[2];

            Start();
        }

        public void Start() {
            if (Room.GameCompleted(Score)) return;

            State = GameState.Bidding;

            foreach (Player player in Players) 
                player.ClearCalls();

            Table = null;

            Dealer = Dealer.Next;
            Current = Dealer.Next;

            Card.Distribute(Players);

            foreach (Player player in Players)
                player.SendMessage("GameStarted", Array.IndexOf(Players, Dealer), player.CardsString());

            Flush();
        }

        public bool Bid(int index, Suit? suit) {
            if (State != GameState.Bidding || Players[index] != Current)
                return false;

            if (suit == null) {
                if (Current == Dealer) return false;
                Current = Current.Next;

            } else {
                Table = new Table(suit.Value, Current);

                foreach (Player player in Players)
                    player.RevealTalon();

                State++;
                Current = Dealer.Next;
            }

            return true;
        }

        public bool Declare(int index, List<int> indexes) {
            if (State != GameState.Declaring || Players[index] != Current)
                return false;

            if (indexes.Any(i => i < 0 || 8 <= i)) return false;

            if (!Current.CreateCalls(indexes)) return false;

            if (Current.Calls.Max.Cards.Count == 8) {
                History.Add(new Round(Current));
                History.Last().Belot(Current, Score[Current.Team]);

                Start();
                return true;
            }

            if (Current == Dealer) {
                Player maxPlayer = Players.Aggregate((a, b) => a.Calls.Gt(b.Calls)? a : b);
                History.Add(new Round(maxPlayer));

                State++;
            }

            Current = Current.Next;
            return true;
        }

        public bool Play(int index, int card, bool bela) {
            if (State != GameState.Playing || Players[index] != Current)
                return false;

            if (card < 0 || Current.Cards.Count <= card) return false;

            if (!Table.Play(Current, index, bela)) return false;

            if (bela) History.Last().Bela(Current);

            if (Table.Complete()) {
                bool last = Players[0].Cards.Count == 0;
                History.Last().Play(Table, last, out Current);

                if (last) Start();

            } else Current = Current.Next;

            return true;
        }
    }
}
