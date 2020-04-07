using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Aleb.GUI {
    class App: Application {
        static App instance;

        public static Window MainWindow => ((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).MainWindow;
        public static IReadOnlyList<Window> Windows => ((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).Windows;
        public static void Shutdown() => ((ClassicDesktopStyleApplicationLifetime)instance.ApplicationLifetime).Shutdown();
        
        public static readonly KeyModifiers ControlKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? KeyModifiers.Meta : KeyModifiers.Control;

        public static string[] Args;

        public static string Host;

        public static void URL(string url) => Process.Start(new ProcessStartInfo() {
            FileName = url,
            UseShellExecute = true
        });

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
            instance = this;

            Styles.Add(new Dark());
        }

        public override void OnFrameworkInitializationCompleted() {
            if (!(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)) throw new ApplicationException("Invalid ApplicationLifetime");

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
    }
}
