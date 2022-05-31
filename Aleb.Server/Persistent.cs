using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Server {
    class Persistent {
        public static readonly string StorePath = Path.Combine(Environment.GetEnvironmentVariable(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "USERPROFILE" : "HOME"
        ), ".alebserver");
        
        static readonly string Header = "ALEB";

        static byte[] CreateHeader() => Encoding.ASCII.GetBytes(Header).Concat(BitConverter.GetBytes(Protocol.Version)).ToArray();

        static uint DecodeHeader(BinaryReader reader) {
            if (!reader.ReadChars(4).SequenceEqual(Header.ToCharArray())) throw new InvalidDataException();
            uint version = reader.ReadUInt32();

            if (version > Protocol.Version) throw new InvalidDataException();

            return version;
        }

        static object locker = new object();
        static bool Reading = false;
        
        public static List<User> ReadUserPool() {
            List<User> ret = new List<User>();

            lock (locker) {
                if (!File.Exists(StorePath))
                    return ret;

                using (FileStream file = File.Open(StorePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(file)) {
                    uint version = DecodeHeader(reader);

                    int n = reader.ReadInt32();

                    try {
                        Reading = true;

                        for (int i = 0; i < n; i++) {
                            User user = User.FromBinary(reader);
                        
                            if (user != null)
                                ret.Add(user);
                        }
                    } catch (Exception) {
                        throw;

                    } finally {
                        Reading = false;
                    }
                }
            }
            
            return ret;
        }

        public static void SaveUserPool(List<User> pool) {
            if (pool == null || Reading) return;

            MemoryStream output = new MemoryStream();
            
            lock (locker) {
                using (BinaryWriter writer = new BinaryWriter(output)) {
                    writer.Write(CreateHeader());

                    writer.Write(pool.Count);

                    pool.ForEach(i => i.ToBinary(writer));
                }

                File.WriteAllBytes(StorePath, output.ToArray());
            }
        }
    }
}
