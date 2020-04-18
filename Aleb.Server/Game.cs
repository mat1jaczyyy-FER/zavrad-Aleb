using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Game {
        public void Flush() {
            foreach (Player player in Players)
                player.Flush();
        }

        void Broadcast(int delay, string command, params dynamic[] args) {
            foreach (Player player in Players)
                player.SendMessage(delay, command, args);
        }

        void Broadcast(string command, params dynamic[] args)
            => Broadcast(0, command, args);

        Player[] Players = new Player[4];

        GameState State;

        Table Table;

        Player Dealer, Current;

        public List<Round> History { get; private set; } = new List<Round>();
        public int[] Score => Enumerable.Range(0, 2).Select(i => History.Where(x => x.Finalized).Sum(x => x.Played[i])).ToArray();

        Room Room;

        public Game(Room room) {
            Room = room;

            Players[0] = Room.Users[0].ToPlayer(this);
            Players[1] = Room.Users[2].ToPlayer(this);
            Players[2] = Room.Users[1].ToPlayer(this);
            Players[3] = Room.Users[3].ToPlayer(this);

            for (int i = 0; i < 4; i++) {
                Players[i].Previous = Players[Utilities.Modulo(i - 1, 4)];
                Players[i].Next = Players[Utilities.Modulo(i + 1, 4)];
                Players[i].Teammate = Players[Utilities.Modulo(i + 2, 4)];
                Players[i].Team = i % 2;
            }

            Dealer = Players[2];

            Start();
        }

        public void Start(int delay = 0) {
            if (Room.GameCompleted(delay)) return;

            State = GameState.Bidding;

            foreach (Player player in Players) 
                player.ClearCalls();

            Table = null;

            Dealer = Dealer.Next;
            Current = Dealer.Next;

            Card.Distribute(Players);

            foreach (Player player in Players)
                player.SendMessage(delay, "GameStarted", Array.IndexOf(Players, Dealer), player.Cards.ToIntString());
        }

        public void Bid(Player sender, Suit? suit) {
            if (State != GameState.Bidding || sender != Current) return;

            if (suit == null) {
                if (Current == Dealer) return;
                Current = Current.Next;

                foreach (Player player in Players)
                    player.SendMessage("TrumpNext");

            } else {
                Table = new Table(suit.Value);
                History.Add(new Round(Current));

                foreach (Player player in Players)
                    player.RevealTalon();

                State++;
                Current = Dealer.Next;

                foreach (Player player in Players)
                    player.SendMessage("TrumpChosen", suit.ToString(), player.Cards.ToIntString());
            }
        }

        public bool Declare(Player player, List<int> indexes) {
            if (State != GameState.Declaring || player != Current)
                return false;

            if (indexes?.Any(i => i < 0 || 8 <= i) == true) return false;

            if (!Current.CreateCalls(indexes)) return false;

            if (Current.Calls.Max.Cards.Count == 8) {
                History.Last().Belot(Current, Score[Current.Team]); //todo implementaj ovo nekako

                Start();
                return true;
            }

            if (Current == Dealer) {
                Player maxPlayer = Players.Aggregate((a, b) => a.Calls.Gt(b.Calls)? a : b);
                int total = History.Last().ApplyCalls(maxPlayer);

                int delay = total != 0? 1500 : 0;
                Broadcast(delay, "WinningDeclaration", Array.IndexOf(Players, maxPlayer), total, maxPlayer.Calls.ToString(), maxPlayer.Teammate.Calls.ToString());
                Broadcast(delay + maxPlayer.DeclarationDelay(), "StartPlayingCards");

                State++;
            }

            Broadcast("PlayerDeclared", Current.Calls.Max.Value);

            Current = Current.Next;
            return true;
        }

        public int PlayCard(Player player, int card, bool bela) {
            if (State != GameState.Playing || player != Current)
                return -1;

            if (card < 0 || Current.Cards.Count <= card) return -1;

            if (!Table.Play(Current, card, bela, out Card played)) return -1;

            if (bela) History.Last().Bela(Current);

            if (Table.Complete()) {
                bool last = Players[0].Cards.Count == 0;
                Round current = History.Last();

                current.Play(Table, last, out Current);

                string round = current.ToString();

                if (last) {
                    current.Finish();
                    Start(3000);

                    Broadcast(2000, "FinalScores", current.ToString());
                    Broadcast(3000, "TotalScore", current.ToString(), string.Join(',', Score));
                
                } else Broadcast(2000, "ContinuePlayingCards", Array.IndexOf(Players, Current));
                
                Broadcast("TableComplete", round, current.Fail);

            } else Current = Current.Next;

            Broadcast("CardPlayed", (int)played, bela);

            return card;
        }
    }
}
