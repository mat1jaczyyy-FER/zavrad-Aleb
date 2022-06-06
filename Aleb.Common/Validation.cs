using System;
using System.Linq;

namespace Aleb.Common {
    public static class Validation {
        static bool Validate(string text, int min, int max, params char[] allowed) {
            if (text.Length < min || max < text.Length) return false;
            if (!text.All(i => allowed.Contains(i) || char.IsLetterOrDigit(i) || i == '_' || i == '-')) return false;
            return true;
        }

        public static bool ValidateUsername(string text) => Validate(text, 4, 18);
        public static bool ValidatePassword(string text) => Validate(text, 8, 32);

        public static bool ValidateRoomName(string text) => Validate(text, 4, 30, ' ');
        public static bool ValidateRoomGoal(int goal) => 50 <= goal && goal <= 10001; // todo set min back to 501
        public static bool ValidateRoomPassword(string text) => text == "" || Validate(text, 6, 32);
    }
}
