using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aleb.Common {
    public class RecordedMatch {
        public string RoomName;
        public int[] Score = new int[2];
        public string[] Users = new string[4];
        public DateTime Date;
        
        public void ReadMetadata(BinaryReader reader) {
            RoomName = reader.ReadString();
            
            for (int i = 0; i < 2; i++)
                Score[i] = reader.ReadInt32();

            for (int i = 0; i < 4; i++)
                Users[i] = reader.ReadString();
            
            Date = DateTime.FromBinary(reader.ReadInt64());
        }

        public void WriteMetadata(BinaryWriter writer) {
            writer.Write(RoomName);
            
            for (int i = 0; i < 2; i++)
                writer.Write(Score[i]);

            for (int i = 0; i < 4; i++)
                writer.Write(Users[i]);            
            
            writer.Write(Date.Ticks);
        }
        
        List<List<List<Message>>> data = new List<List<List<Message>>>();

        public void NewRound()
            => data.Add(new List<List<Message>>());

        public void WriteRecords(List<Message> records)
            => data.Last().Add(records);
        
        public static RecordedMatch FromMetadata(BinaryReader reader) {
            RecordedMatch match = new RecordedMatch();

            match.ReadMetadata(reader);

            return match;
        }

        public static RecordedMatch FromBinary(BinaryReader reader) {
            RecordedMatch match = new RecordedMatch();

            match.ReadMetadata(reader);

            int a = reader.ReadInt32();
            for (int i = 0; i < a; i++) {
                match.NewRound();
                int b = reader.ReadInt32();
                for (int j = 0; j < b; j++) {
                    int c = reader.ReadInt32();
                    List<Message> records = new List<Message>();
                    for (int k = 0; k < c; k++)
                        records.Add(Message.Parse(reader.ReadString()));
                    match.WriteRecords(records);
                }
            }

            return match;
        }

        public void ToBinary(BinaryWriter writer) {
            WriteMetadata(writer);

            writer.Write(data.Count);
            data.ForEach(round => {
                writer.Write(round.Count);
                round.ForEach(records => {
                    writer.Write(records.Count);
                    records.ForEach(record => {
                        writer.Write(record.ToString().Replace("\n", ""));
                    });
                });
            });
        }
    }
}
