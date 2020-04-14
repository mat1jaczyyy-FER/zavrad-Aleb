using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleb.Server {
    static class EnumUtil {
        public static IEnumerable<T> Values<T>() where T: Enum
            => Enum.GetValues(typeof(T)).Cast<T>();

        public static int CallValue(this Value value) {
            if (value == Value.Jack) return 200;
            if (value == Value.IX) return 150;
            if (value <= Value.VIII) return 0;
            return 100;
        }

        static Value[] Trumps = new Value[] { Value.VII, Value.VIII, Value.Queen, Value.King, Value.X, Value.Ace, Value.IX, Value.Jack };
        static Value[] NonTrumps = new Value[] { Value.VII, Value.VIII, Value.IX, Value.Jack, Value.Queen, Value.King, Value.X, Value.Ace };

        static int Index(Value value, bool trump) => Array.IndexOf(trump? Trumps : NonTrumps, value);

        public static bool Gt(this Value value, Value other, bool trump) => Index(value, trump) >= Index(other, trump);

        static int[] Points = new int[] { 0, 2, 3, 4, 10, 11, 14, 20 };

        public static int ToPoints(this Value value, bool trump) {
            if (value <= Value.VIII) return 0;
            return Points[Index(value, trump) - (trump? 0 : 2)];
        }

        public static string ToIntString(this List<Card> cards)
            => string.Join(',', cards.Select(i => (int)i));
    }
}
