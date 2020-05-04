using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;

using Avalonia;

namespace Aleb.GUI {
    static class Program {
        public static readonly string Version = "Alpha Build 3";

        public static Stopwatch TimeSpent = new Stopwatch();

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect();

        public static readonly string UserPath = Path.Combine(Environment.GetEnvironmentVariable(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "USERPROFILE" : "HOME"
        ), ".aleb");

        public static readonly string CrashDir = Path.Combine(UserPath, "Crashes");

        [STAThread]
        static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => {
                if (!Directory.Exists(CrashDir)) Directory.CreateDirectory(CrashDir);
                
                using (MemoryStream memoryStream = new MemoryStream()) {
                    string crashName = Path.Combine(CrashDir, $"Crash-{DateTimeOffset.Now.ToUnixTimeSeconds()}");

                    using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                        string additional = "";

                        using (Stream log = archive.CreateEntry("exception.log").Open())
                            using (StreamWriter writer = new StreamWriter(log)) {
                                writer.Write(
                                    $"Aleb Version: {Version}\r\n" +
                                    $"Operating System: {RuntimeInformation.OSDescription}\r\n\r\n" +
                                    e.ExceptionObject.ToString() +
                                    additional
                                );
                            }
                    }

                    File.WriteAllBytes(crashName + ".zip", memoryStream.ToArray());
                }
            };
            
            TimeSpent.Start();
            App.Args = args;
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(null);
            
            Preferences.Save();
        }
    }
}
