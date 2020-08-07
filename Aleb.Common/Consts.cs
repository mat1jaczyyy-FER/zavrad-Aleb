namespace Aleb.Common {
    public enum Suit {
        Hearts, Leaves, Bells, Acorns
    }

    public enum UserState {
        Idle, InRoom, InGame   
    }

    public enum GameType {
        Dosta,
        Prolaz
    }

    public enum GameState {
        Bidding,
        Declaring,
        Playing
    }

    public static class Consts {
        public const int BelotValue = int.MaxValue / 3;
    }
}
