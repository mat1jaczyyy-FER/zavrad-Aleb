using System;
using System.Collections.Generic;
using System.Text;

namespace Aleb.Common {
    public enum Suit {
        Hearts, Leaves, Bells, Acorns
    }

    public enum UserState {
        Idle, InRoom, InGame   
    }

    public enum GameState {
        Bidding,
        Declaring,
        Playing
    }
}
