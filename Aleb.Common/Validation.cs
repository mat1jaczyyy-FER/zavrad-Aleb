using System;
using System.Linq;

namespace Aleb.Common {
    public static class Validation {
        static bool Validate(string name, int min, int max, params char[] allowed) {
            if (name.Length < min || max < name.Length) return false;
            if (!name.All(i => allowed.Contains(i) || char.IsLetterOrDigit(i) || i == '_' || i == '-')) return false;
            return true;
        }

        public static bool ValidateUsername(string name) => Validate(name, 4, 18);
        public static bool ValidatePassword(string name) => Validate(name, 8, 32);

        public static bool ValidateRoomName(string name) => Validate(name, 4, 30, ' ');
    }
}
