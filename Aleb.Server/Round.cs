using System.Linq;

namespace Aleb.Server {
    class Round {
        public int[] Calls { get; private set; } = new int[2];
        public int[] Played { get; private set; } = new int[2];

        int Team;
        bool Fail;
        public bool Finalized { get; private set; }

        public Round(Player caller)
            => Calls[Team = caller.Team] = caller.Calls.Total + caller.Teammate.Calls.Total;

        public void Bela(Player caller) => Calls[caller.Team] += 20;

        public void Play(Table table, bool last, out Player winner) {
            Played[table.Winner.Player.Team] += table.Points + (last? 10 : 0);

            winner = table.Winner.Player;
            table.Clear();

            if (last) {
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
                Fail = Played[Team] <= Played[1 - Team];
                Finalized = true;
            }
        }

        public void Belot(Player caller, int currentScore) {
            Calls = new int[2];
            Played[caller.Team] += 1001 - currentScore;
            Finalized = true;
        }
    }
}
