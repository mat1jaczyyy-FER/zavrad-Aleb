using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Aleb.Common;

namespace Aleb.Client {
    public class Profile {
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
        public readonly List<Tuple<string, string>> Statistics;
        public readonly List<RecordedMatch> MatchHistory;

        public UserState State;

        public Profile(string raw) {
            string[] args = raw.Split('|');

            int i = 0;
            Name = args[i++];

            Statistics = new List<Tuple<string, string>>();
            
            foreach (string key in StatList.Select(i => i.Item1))
                Statistics.Add(new Tuple<string, string>(key, args[i++]));
            
            MatchHistory = new List<RecordedMatch>();

            using (MemoryStream ms = new MemoryStream(Convert.FromHexString(args[i++])))
            using (BinaryReader reader = new BinaryReader(ms)) {
                int n = reader.ReadInt32();

                for (int j = 0; j < n; j++)
                    MatchHistory.Add(RecordedMatch.FromMetadata(reader));
            }
        }
    }
}
