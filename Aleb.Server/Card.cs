using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    enum Suit {
        Hearts, Leaves, Bells, Acorns
    }

    enum Value {
        VII, VIII, IX, X, Jack, Queen, King, Ace
    }

    class Card {
        static Random RNG = new Random();

        static void Swap<T>(List<T> list, int a, int b) {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        public static void Distribute(Player[] players) {
            List<Card> cards = new List<Card>();

            foreach (Suit suit in EnumUtil.Values<Suit>())
                foreach (Value value in EnumUtil.Values<Value>())
                    cards.Add(new Card(suit, value));

            for (int i = cards.Count; i >= 0; i--)
                Swap(cards, i, RNG.Next(i + 1));

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

        public static bool operator <(Card a, Card b)
            => a.Suit == b.Suit
                ? a.Value < b.Value
                : a.Suit < b.Suit;

        public static bool operator >(Card a, Card b)
            => a.Suit == b.Suit
                ? a.Value > b.Value
                : a.Suit > b.Suit;

        public bool Gt(Card other, Suit trump) {
            if (Suit == trump && other.Suit == trump) return Value.Gt(other.Value, true);
            if (Suit == trump || other.Suit == trump) return Suit == trump;
            return Value.Gt(other.Value, false);
        }
    }
}
