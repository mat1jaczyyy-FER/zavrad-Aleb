using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aleb.Common {
    public static class Utilities {
        public static int Modulo(int n, int m) => (n + m) % m;

        public static void Swap<T>(this List<T> list, int a, int b) {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        public static IEnumerable<T> Rotate<T>(this IEnumerable<T> enumerable, int amount)
            => enumerable.Skip(amount).Concat(enumerable.Take(amount));

        public static IEnumerable<T> RotateWith<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) {
            int index = 0;

            foreach (T item in enumerable) {
                if (predicate.Invoke(item)) break;
                index++;
            }

            return enumerable.Rotate(index);
        }
    }
}
