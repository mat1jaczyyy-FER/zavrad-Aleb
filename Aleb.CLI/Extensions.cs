using System;
using System.Collections.Generic;
using System.Text;

using Aleb.Client;

namespace Aleb.CLI {
    static class Extensions {
        public static string Display(this Room room) => $"{room.Name}, Players: {room.Count}/4";
    }
}
