using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Client {
    public class UserStats {
        public readonly string Name;
        public static readonly List<Tuple<string, string>> StatList = new List<Tuple<string, string>>() {
            new Tuple<string, string>("PointsScored", "Sveukupno bodova"),
            new Tuple<string, string>("GamesPlayed", "Partija odigrano"),
            new Tuple<string, string>("GamesWon", "Partija pobjedio"),
            new Tuple<string, string>("GamesLost", "Partija izgubio"),
            new Tuple<string, string>("Bidded", "Zvao puta"),
            new Tuple<string, string>("BidSuccesses", "Prošao puta"),
            new Tuple<string, string>("BidFailures", "Pao puta"),
            new Tuple<string, string>("Calls20", "20 zvanja"),
            new Tuple<string, string>("Calls50", "50 zvanja"),
            new Tuple<string, string>("Calls100", "100 zvanja"),
            new Tuple<string, string>("Calls150", "150 zvanja"),
            new Tuple<string, string>("Calls200", "200 zvanja"),
            new Tuple<string, string>("SixRow", "6 karata u nizu"),
            new Tuple<string, string>("SevenRow", "7 karata u nizu"),
            new Tuple<string, string>("Belotes", "8 karata u nizu"),    
            new Tuple<string, string>("MaxPointsRound", "Najviše bodova u rundi"),
            new Tuple<string, string>("MaxPointsMatch", "Najviše bodova u partiji"),
        };
        public readonly List<Tuple<string, string>> Dict;

        public UserState State;

        public UserStats(string raw) {
            string[] args = raw.Split('|');

            Name = args[0];
            Dict = new List<Tuple<string, string>>();
            int i = 1;

            foreach (string key in StatList.Select(i => i.Item1))
                Dict.Add(new Tuple<string, string>(key, args[i++]));
        }
    }
}
