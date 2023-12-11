using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static T? ToEnum<T>(this string str) where T: struct, Enum
            => Enum.TryParse(str, false, out T value)? value : (T?)null;

        public static string ToStr<T>(this IEnumerable<T> list, Func<T, string> converter = null, char delimiter = Protocol.ListDelimiter)
            => string.Join(delimiter, list.Select(converter?? (i => i.ToString())));

        public static List<T> ToList<T>(this string str, Func<string, T> converter, char delimiter = Protocol.ListDelimiter)
            => (str == null || str == "")? new List<T>() : str.Split(delimiter).Select(converter).ToList();

        public static List<int> ToIntList(this string str, char delimiter = Protocol.ListDelimiter)
            => str.ToList(i => Convert.ToInt32(i), delimiter);

        public static Task FireAndForget(Action action)
            => Task.Run(action).ContinueWith(t => {
                if (t.IsFaulted)
                    Console.Error.WriteLine(t.Exception);
            });
    }
}
