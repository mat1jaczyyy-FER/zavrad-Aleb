using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Round {
        public int[] Calls { get; private set; } = new int[2];
        public int[] Played { get; private set; } = new int[2];
        public int[] Taken { get; private set; } = new int[2];

        public int Team { get; private set; }
        public bool Fail { get; private set; }
        public bool Finalized { get; private set; }
        public Suit Suit { get; private set; }

        public Round(Player bidder, Suit suit) {
            Team = bidder.Team;
            Suit = suit;
        }

        public int ApplyCalls(Player caller)
            => Calls[caller.Team] = caller.Calls.Total + caller.Teammate.Calls.Total;

        public void Bela(Player caller) => Calls[caller.Team] += 20;

        public void Play(Table table, bool last, out Player winner) {
            Played[table.Winner.Player.Team] += table.Points + (last? 10 : 0);
            Taken[table.Winner.Player.Team]++;

            winner = table.Winner.Player;
            table.Clear();
        }

        public void Finish(bool last) {
            bool capot = false;

            if (last)
                for (int i = 0; i < 2; i++)
                    if (Taken[i] == 0) {
                        Played[1 - i] += 90 + Calls.Sum();
                        capot = true;
                        break;
                    }

            if (!capot)
                for (int i = 0; i < 2; i++)
                    Played[i] += Played[i] > 0? Calls[i] : 0;

            Calls = new int[2];
            
            if (last && (Fail = Played[Team] <= Played[1 - Team])) {
                Played[1 - Team] += Played[Team];
                Played[Team] = 0;
            }

            Finalized = true;
        }

        public void Belot(Player caller, int currentScore, int goal) {
            Calls = new int[2];
            Played[caller.Team] += goal - currentScore;
            Finalized = true;
        }

        string PlayedString => $"{Played.ToStr(delimiter: ',')}";

        public string ToStringNoFail() => Finalized
            ? PlayedString
            : $"{Calls.ToStr(delimiter: ',')};{PlayedString}";

        public override string ToString() => $"{ToStringNoFail()},{Suit},{Fail}";
    }
}
