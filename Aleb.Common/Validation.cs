using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aleb.Common {
    public static class Validation {
        public static bool Validate(string name, int maxLen) {
            if (name.Length < 4 || maxLen < name.Length) return false;
            if (!name.All(i => char.IsLetterOrDigit(i) || i == '_' || i == '-')) return false;
            return true;
        }

        public static bool ValidateUserName(string name) => Validate(name, 18);
        public static bool ValidateRoomName(string name) => Validate(name, 30);
    }
}
