using Aleb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    class Call: IComparable<Call> {
        public readonly int Value;
        public readonly List<Card> Cards;

        public Call(int value, IEnumerable<Card> cards) {
            Value = value;
            Cards = cards.ToList();
            Cards.Sort();
        }

        public int CompareTo(Call other) => Gt(other)? 1 : -1;

        public bool Gt(Call other) {
            if (Value != other.Value) return Value > other.Value;
            if (Value == 0) return true;

            if (Value >= 100) {
                if (Cards.Count == 4 && other.Cards.Count == 4)
                    return Cards[0].Value.Gt(other.Cards[0].Value, true);

                if (Cards.Count == 4 || other.Cards.Count == 4)
                    return Cards.Count < other.Cards.Count;
            }

            return Cards.Last().Value >= other.Cards.Last().Value;
        }
    }

    class Calls {
        public Call Max => IndividualCalls.LastOrDefault()?? new Call(0, Enumerable.Empty<Card>());
        public int Total => IndividualCalls.Sum(i => i.Value);

        HashSet<Card> Cards = new HashSet<Card>();
        public int Used => Cards.Count;

        List<Call> IndividualCalls = new List<Call>();
        public bool IsBelot => Max.Value >= Consts.BelotValue;

        public void Add(int value, IEnumerable<Card> cards) {
            foreach (Card card in cards)
                Cards.Add(card);

            IndividualCalls.Add(new Call(value, cards));
            IndividualCalls.Sort();
        }

        public bool Gt(Calls other) => Max.Gt(other.Max);

        public override string ToString() {
            List<Card> cards = Cards.ToList();
            cards.Sort();

            return cards.ToStr(); // todo check
        }
    }
}
