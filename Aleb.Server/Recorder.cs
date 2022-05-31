using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Recorder {
        protected List<User> Users { get; private set; }

        public void SendMessage(int delay, string command, params dynamic[] args) {
            Message msg = new Message(command, args);

            foreach (User user in Users)
                if (user?.Client?.Connected == true)
                    user.Client.Send(delay, new Message(command, args));

            if (Record) TempRecords.Add(msg);
        }

        public void SendMessage(string command, params dynamic[] args)
            => SendMessage(0, command, args);

        public void Flush() {
            foreach (User user in Users)
                if (user?.Client?.Connected == true)
                    user.Client.Flush();
            
            TempRecords.Reverse();
            Records = Records.Concat(TempRecords).ToList();
            TempRecords.Clear();
        }

        protected bool Record = true;
        List<Message> Records = new List<Message>();
        List<Message> TempRecords = new List<Message>();

        public void ClearRecords() {
            Records.Clear();
            TempRecords.Clear();
        }
        
        public void ReplayRecords(User user) {
            if (Users.Contains(user) && user?.Client?.Connected != true) return;
            
            Records.Reverse();

            foreach (Message msg in Records)
                user.Client.Send(0, msg);

            user.Client.Flush();

            Records.Reverse();
        }

        public Recorder(List<User> users) => Users = users;
    }
}
