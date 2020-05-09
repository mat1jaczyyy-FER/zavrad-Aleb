using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Newtonsoft.Json;

using Aleb.Client;

namespace Aleb.GUI {
    class App: Application {
        public static App instance;

        public static AlebWindow MainWindow => (AlebWindow)((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).MainWindow;
        public static IReadOnlyList<Window> Windows => ((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).Windows;
        public static void Shutdown() => ((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).Shutdown();
        
        public static readonly KeyModifiers ControlKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? KeyModifiers.Meta : KeyModifiers.Control;

        public static bool Dragging;

        public static string[] Args;

        public static CultureInfo Culture = new CultureInfo("hr-HR");

        public static string Host;
        public static User User;

        public static void URL(string url) => Process.Start(new ProcessStartInfo() {
            FileName = url,
            UseShellExecute = true
        });

        public static Bitmap GetImage(string uri)
            => new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri($"avares://Aleb.GUI/Assets/{uri}.png")));

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
            instance = this;

            Styles.Add(new Dark());
        }

        public override void OnFrameworkInitializationCompleted() {
            if (!(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)) throw new ApplicationException("Invalid ApplicationLifetime");

            #if !DEBUG
                Host = "40.114.147.48";
            #endif

            if (Args.Length == 2 && Args[0] == "--host") Host = Args[1];
            else if (Args.Length != 0) {
                Console.Error.WriteLine("Invalid arguments.");
                return;
            }

            if (Preferences.DiscordPresence) Discord.Set(true);

            lifetime.Exit += (_, __) => Discord.Set(false);

            lifetime.MainWindow = new AlebWindow();
            base.OnFrameworkInitializationCompleted();
        }
        
        static readonly string DepsPath = $"{AppDomain.CurrentDomain.BaseDirectory}Aleb.deps.json";
        static string avaloniaVersion = "";

        public static string AvaloniaVersion() {
            if (avaloniaVersion == "" && File.Exists(DepsPath)) {
                try {
                    using (StreamReader file = File.OpenText(DepsPath))
                        using (JsonTextReader reader = new JsonTextReader(file))
                            while (reader.Read())
                                if (reader.TokenType == JsonToken.String &&
                                    reader.Path.StartsWith("targets['.NETCoreApp,Version=v3.1") &&
                                    reader.Path.EndsWith("']['Aleb/0.0.0'].dependencies.Avalonia")) {
                                        
                                    avaloniaVersion = (string)reader.Value;
                                    break;
                                }
                } catch {
                    avaloniaVersion = "";
                }
            }

            return avaloniaVersion;
        }
    }
}
