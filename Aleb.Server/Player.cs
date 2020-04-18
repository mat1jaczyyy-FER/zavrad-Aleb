using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Player {
        List<Card> _cards;
        public List<Card> Cards {
            get => _cards;
            set => (_cards = value).Sort();
        }

        public List<Card> Talon;

        public void RevealTalon() {
            Cards = Cards.Concat(Talon).ToList();
            Talon = null;
        }

        public Calls Calls { get; private set; }

        public void ClearCalls() => Calls = null;

        public bool CreateCalls(List<int> indexes) { // todo rename sve calls u declarations
            Calls calls = new Calls();

            if (indexes != null) {
                if (!indexes.Any()) return false;

                List<Card> cards = Cards.Where((x, i) => indexes.Contains(i)).ToList();

                foreach (Value value in EnumUtil.Values<Value>().Where(i => i.CallValue() > 0)) {
                    IEnumerable<Card> filtered = cards.Where(i => i.Value == value);

                    if (filtered.Count() == 4)
                        calls.Add(value.CallValue(), filtered);
                }

                int start = 0;

                for (int i = 1; i <= cards.Count; i++) {
                    if (i == cards.Count || !cards[i - 1].IsNext(cards[i])) {
                        int count = i - start;

                        if (count >= 3)
                            calls.Add(Math.Min(100, 10 * count * count - 40 * count + 50), cards.Skip(start).Take(count));

                        start = i;
                    }
                }

                if (calls.Used < cards.Count) return false;
            }
        
            Calls = calls;
            return true;
        }

        double DeclarationLog(double x) => Math.Log(2 * x + 1, 1.0013);

        public int DeclarationDelay() {
            double a = DeclarationLog(Calls.Used);
            double b = DeclarationLog(Teammate.Calls.Used);

            return (int)(1500 + Math.Max(a, b) + Math.Pow(a * b, 0.45));
        }

        public Player Previous, Next, Teammate;
        public int Team;
        
        public User User { get; private set; }

        public void SendMessage(int delay, string command, params dynamic[] args) {
            if (User?.Client?.Connected == true)
                User.Client.Send(delay, new Message(command, args));
        }

        public void SendMessage(string command, params dynamic[] args)
            => SendMessage(0, command, args);

        public void Flush() {
            if (User?.Client?.Connected == true)
                User.Client.Flush();
        }

        public Player(User user) => User = user;
    }
}
