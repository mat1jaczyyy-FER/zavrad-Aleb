using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Client {
    public class FinalizedRound {
        public readonly List<int> Score;
        public readonly bool Fail;
        public readonly Suit Suit;

        public FinalizedRound(string raw) {
            string[] args = raw.Split(',');

            Score = args.Take(2).Select(i => Convert.ToInt32(i)).ToList();
            Suit = args[2].ToEnum<Suit>().Value;
            Fail = Convert.ToBoolean(args[3]);
        }
    }
}
