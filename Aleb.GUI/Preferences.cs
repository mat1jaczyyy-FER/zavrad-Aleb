using System;
using System.Linq;
using System.IO;

namespace Aleb.GUI {
    static class Preferences {
        const int Version = 1;
        static readonly char[] Header = new char[] {'A', 'L', 'E', 'B'};

        static readonly string FilePath = Path.Combine(Program.UserPath, "Aleb.config");
        static readonly string StatsPath = Path.Combine(Program.UserPath, "Aleb.stats");

        public delegate void SimpleBoolChangedEventHandler(bool newValue);
        public delegate void ChangedEventHandler();

        static bool _DiscordPresence = true;
        public static bool DiscordPresence {
            get => _DiscordPresence;
            set {
                _DiscordPresence = value;
                Discord.Set(DiscordPresence);
                Save();
            }
        }

        public static event SimpleBoolChangedEventHandler MiViChanged;
        static bool _MiVi = true;
        public static bool MiVi {
            get => _MiVi;
            set {
                _MiVi = value;
                MiViChanged?.Invoke(MiVi);
                Save();
            }
        }

        static bool _Top = true;
        public static bool Topmost {
            get => _Top;
            set {
                _Top = value;

                if (App.MainWindow != null)
                    App.MainWindow.Topmost = _Top;

                Save();
            }
        }

        public static long BaseTime = 0;
        public static long Time => BaseTime + (long)Program.TimeSpent.Elapsed.TotalSeconds;
        
        static bool Decoding;

        static void Decode(string path, Action<BinaryReader> decoder) {
            if (!File.Exists(path)) return;

            Decoding = true;

            using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read))
                try {
                    using (BinaryReader reader = new BinaryReader(file)) {
                        if (!reader.ReadBytes(4).Select(i => (char)i).SequenceEqual(Header))
                            throw new InvalidDataException();

                        decoder?.Invoke(reader);
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine("Error reading Preferences");
                    Console.Error.WriteLine(e.ToString());
                }

            Decoding = false;
        }

        static void Encode(string path, Action<BinaryWriter> encoder) {
            if (Decoding) return;

            if (!Directory.Exists(Program.UserPath)) Directory.CreateDirectory(Program.UserPath);

            MemoryStream output = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(output)) {
                writer.Write(Header);

                encoder?.Invoke(writer);
            }

            try {
                File.WriteAllBytes(path, output.ToArray());
            } catch (Exception e) {
                Console.Error.WriteLine("Error writing Preferences");
                Console.Error.WriteLine(e.ToString());
            }
        }

        static Preferences() {
            Decode(FilePath, reader => {
                int version = reader.ReadInt32();
                if (version > Version)
                    throw new InvalidDataException();

                DiscordPresence = reader.ReadBoolean();

                MiVi = reader.ReadBoolean();

                if (version >= 1)
                    Topmost = reader.ReadBoolean();
            });

            Decode(StatsPath, reader => BaseTime = reader.ReadInt64());
        }

        public static void Save() {
            Encode(FilePath, writer => {
                writer.Write(Version);

                writer.Write(DiscordPresence);

                writer.Write(MiVi);

                writer.Write(Topmost);
            });

            Encode(StatsPath, writer => writer.Write(Preferences.Time));
        }
    }
}