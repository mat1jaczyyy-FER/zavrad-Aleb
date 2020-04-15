using System.Linq;

namespace Aleb.Server {
    class Round {
        public int[] Calls { get; private set; } = new int[2];
        public int[] Played { get; private set; } = new int[2];

        public int Team { get; private set; }
        bool Fail;
        public bool Finalized { get; private set; }

        public Round(Player bidder)
            => Team = bidder.Team;

        public int ApplyCalls(Player caller)
            => Calls[caller.Team] = caller.Calls.Total + caller.Teammate.Calls.Total;

        public void Bela(Player caller) => Calls[caller.Team] += 20;

        public void Play(Table table, bool last, out Player winner) {
            Played[table.Winner.Player.Team] += table.Points + (last? 10 : 0);

            winner = table.Winner.Player;
            table.Clear();
        }

        public void Finish() {
            bool capot = false;

            for (int i = 0; i < 2; i++)
                if (Played[i] == 0) {
                    Played[i] += 90 + Calls.Sum();
                    capot = true;
                    break;
                }

            if (!capot)
                for (int i = 0; i < 2; i++)
                    Played[i] += Calls[i];

            Calls = new int[2];
            
            if (Fail = Played[Team] <= Played[1 - Team]) {
                Played[1 - Team] += Played[Team];
                Played[Team] = 0;
            }

            Finalized = true;
        }

        public void Belot(Player caller, int currentScore) {
            Calls = new int[2];
            Played[caller.Team] += 1001 - currentScore;
            Finalized = true;
        }

        public override string ToString() => Finalized
            ? $"{string.Join(',', Played)};{Fail}"
            : $"{string.Join(',', Calls)};{string.Join(',', Played)}";
    }
}
