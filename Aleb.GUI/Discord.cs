using System;
using System.Timers;

using DiscordRPC;

namespace Aleb.GUI {
    static class Discord {
        static bool Initialized = false;
        static DiscordRpcClient Presence;
        
        static Timer courier = new Timer() { Interval = 1000 };
        static object locker = new object();

        public static readonly Assets Logo = new Assets() { LargeImageKey = "logo" };

        static RichPresence _info = new RichPresence();
        public static RichPresence Info {
            get => _info;
            set {
                _info = value;
                Logo.SmallImageKey = null;
                Logo.SmallImageText = null;
            }
        }

        static Discord() => courier.Elapsed += Refresh;

        public static void Set(bool state) {
            if (state) Init();
            else Dispose();
        }

        static void Init() {
            if (Initialized) return;

            Presence = new DiscordRpcClient("696874859418091622");
            Presence.Initialize();
            Initialized = true;

            Refresh();
            courier.Start();
        }

        static void Refresh(object sender = null, EventArgs e = null) {
            lock (locker) {
                if (Initialized)
                    Presence.SetPresence(Info.WithAssets(Logo));
            }
        }

        static void Dispose() {
            if (!Initialized) return;

            lock (locker) {
                courier.Stop();

                Initialized = false;
                Presence.ClearPresence();
                Presence.Dispose();
            }
        }
    }
}