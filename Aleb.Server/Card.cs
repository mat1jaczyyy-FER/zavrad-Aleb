using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    enum Value {
        VII, VIII, IX, X, Jack, Queen, King, Ace
    }

    class Card: IComparable<Card> {
        static Random RNG = new Random();

        public static void Distribute(Player[] players) {
            List<Card> cards = new List<Card>();

            foreach (Suit suit in EnumUtil.Values<Suit>())
                foreach (Value value in EnumUtil.Values<Value>())
                    cards.Add(new Card(suit, value));

            for (int i = cards.Count - 1; i >= 0; i--)
                cards.Swap(i, RNG.Next(i + 1));

            for (int i = 0; i < 4; i++) {
                players[i].Cards = cards.Skip(i * 8).Take(6).ToList();
                players[i].Talon = cards.Skip(i * 8 + 6).Take(2).ToList();
            }
        }

        public readonly Suit Suit;
        public readonly Value Value;

        public int Points(Suit trump) => Value.ToPoints(Suit == trump);

        public Card(Suit suit, Value value) {
            Suit = suit;
            Value = value;
        }

        public bool IsNext(Card other)
            => other.Suit == Suit && other.Value - 1 == Value;

        public static implicit operator int(Card card) => (int)card.Suit * 8 + (int)card.Value;

        public override bool Equals(object obj) {
            if (!(obj is Card)) return false;
            return this == (Card)obj;
        }

        public static bool operator ==(Card a, Card b) {
            if (a is null || b is null) return ReferenceEquals(a, b);
            return a.Suit == b.Suit && a.Value == b.Value;
        }
        public static bool operator !=(Card a, Card b) => !(a == b);

        public override int GetHashCode() => HashCode.Combine(Suit, Value);
        
        public int CompareTo(Card other) {
            if (this == other) return 0;

            bool result = Suit == other.Suit
                ? Value < other.Value
                : Suit < other.Suit;

            return result? -1 : 1;
        }

        public bool Gt(Card other, Suit trump, Suit first) {
            if (Suit == trump && other.Suit == trump) return Value.Gt(other.Value, true);
            if (Suit == trump || other.Suit == trump) return Suit == trump;

            if (Suit == first && other.Suit == first) return Value.Gt(other.Value, false);
            if (Suit == first || other.Suit == first) return Suit == first;

            return Value.Gt(other.Value, false);
        }
    }
}
