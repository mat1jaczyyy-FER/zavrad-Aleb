using NetCoreAudio;

namespace Aleb.GUI {
    static class Audio {
        static Player Player = new Player();

        public static void YourTurn() {
            if (Preferences.Notify.ShouldNotify())
                Player.Play("Audio/YourTurn.wav");
        }

        public static void Fail() {
            if (Preferences.Notify != Preferences.NotificationType.Never)
                Player.Play("Audio/Fail.wav");
        }
    }
}
