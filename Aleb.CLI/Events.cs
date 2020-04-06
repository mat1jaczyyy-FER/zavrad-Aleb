using System;
using System.Collections.Generic;
using System.Text;

using Aleb.Client;

namespace Aleb.CLI {
    static class Events {
        public static void RoomAdded(Room room)
            => RoomMenus.Rooms.Add(room);
    }
}
