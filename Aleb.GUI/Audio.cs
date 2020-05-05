using NetCoreAudio;

namespace Aleb.GUI {
    static class Audio {
        static Player Player = new Player();

        public static void YourTurn() {
            if (!App.MainWindow.IsActive) {
                App.MainWindow.BringToTop();
                Player.Play("Audio/YourTurn.wav");
            }
        }
    }
}
