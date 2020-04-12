using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Aleb.GUI {
    static class Preferences {
        const int Version = 0;
        static readonly char[] Header = new char[] {'A', 'L', 'E', 'B'};

        static readonly string FilePath = Path.Combine(Program.UserPath, "Aleb.config");

        public delegate void CheckBoxChanged(bool newValue);
        public delegate void Changed();

        static bool _DiscordPresence = true;
        public static bool DiscordPresence {
            get => _DiscordPresence;
            set {
                _DiscordPresence = value;
                Discord.Set(DiscordPresence);
                Save();
            }
        }

        public static void Decode(Stream input) {
            using (BinaryReader reader = new BinaryReader(input)) {
                if (!reader.ReadBytes(4).Select(i => (char)i).SequenceEqual(Header))
                    throw new InvalidDataException();

                int version = reader.ReadInt32();
                if (version > Version)
                    throw new InvalidDataException();

                DiscordPresence = reader.ReadBoolean();
            }
        }
        
        public static void Save() {
            if (!Directory.Exists(Program.UserPath)) Directory.CreateDirectory(Program.UserPath);

            MemoryStream output = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(output)) {
                writer.Write(Header);

                writer.Write(DiscordPresence);
            }

            try {
                File.WriteAllBytes(FilePath, output.ToArray());
            } catch (IOException) {}
        }

        static Preferences() {
            if (!File.Exists(FilePath)) return;

            using (FileStream file = File.Open(FilePath, FileMode.Open, FileAccess.Read))
                try {
                    using (BinaryReader reader = new BinaryReader(file)) {
                        if (!reader.ReadBytes(4).Select(i => (char)i).SequenceEqual(Header))
                            throw new InvalidDataException();

                        int version = reader.ReadInt32();
                        if (version > Version)
                            throw new InvalidDataException();

                        DiscordPresence = reader.ReadBoolean();
                    }
                } catch {
                    Console.Error.WriteLine("Error reading Preferences");
                }
        }
    }
}