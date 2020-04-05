using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aleb.Common {
    public static class Validation {
        public static bool Validate(string name, int min, int max) {
            if (name.Length < min || max < name.Length) return false;
            if (!name.All(i => char.IsLetterOrDigit(i) || i == '_' || i == '-')) return false;
            return true;
        }

        public static bool ValidateUsername(string name) => Validate(name, 4, 18);
        public static bool ValidatePassword(string name) => Validate(name, 8, 32);

        public static bool ValidateRoomName(string name) => Validate(name, 4, 30);
    }
}
