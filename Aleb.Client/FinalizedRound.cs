using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleb.Client {
    public class FinalizedRound {
        public readonly List<int> Score;
        public readonly bool Fail;

        public FinalizedRound(string raw) {
            string[] args = raw.Split(',');

            Score = args.Take(2).Select(i => Convert.ToInt32(i)).ToList();
            Fail = Convert.ToBoolean(args[2]);
        }
    }
}
