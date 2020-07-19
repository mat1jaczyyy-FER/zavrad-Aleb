using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class ResultsView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Victory = this.Get<TextBlock>("Victory");

            Score = new TextBlock[] {
                this.Get<TextBlock>("ScoreLeft"),
                this.Get<TextBlock>("ScoreRight")
            };

            Users = this.Get<StackPanel>("UsersLeft").Children.OfType<UserInList>()
                .Concat(this.Get<StackPanel>("UsersRight").Children.OfType<UserInList>()).ToArray();
        }

        Room Room;

        TextBlock Victory;
        TextBlock[] Score;
        UserInList[] Users;

        public ResultsView() => throw new InvalidOperationException();
        
        public ResultsView(List<int> score, Room room) {
            InitializeComponent();

            Room = room;

            int rotate = room.Users.Select(i => i.Name).ToList().IndexOf(App.User.Name) >> 1 & 1;

            string win = (score[rotate] > score[1 - rotate])? "Pobijedili" : "Izgubili";
            Victory.Text = $"{win} ste"; //todo nerijeseno

            foreach (var (text, pts) in Score.Zip(score.Rotate(rotate)))
                text.Text = pts.ToString();

            foreach (var (text, user) in Users.Zip(room.Users.Rotate(rotate * 2)))
                text.Text = user.Name;

            Discord.Info = new DiscordRPC.RichPresence() {
                Details = win,
                State = $"{Score[0].Text} - {Score[1].Text}"
            };
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined += UserJoined;
            Network.UserReady += UserReady;
            Network.UserLeft += UserLeft;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined -= UserJoined;
            Network.UserReady -= UserReady;
            Network.UserLeft -= UserLeft;
        }

        void UserJoined(User user) {
            int index = Room.Users.ToList().IndexOf(null);

            if (index != -1)
                Room.Users[index] = user;
        }

        void UserReady(User user) {
            User found = Room.Users.FirstOrDefault(i => i.Name == user.Name);

            if (found != null)
                found.Ready = user.Ready;
        }

        void UserLeft(User user) {
            User found = Room.Users.FirstOrDefault(i => i.Name == user.Name);

            if (found != null) {
                for (int i = Room.Users.ToList().IndexOf(found); i < 3; i++)
                    Room.Users[i] = Room.Users[i + 1];
                
                Room.Users[3] = null;
            }
        }

        void BackToRoom(object sender, RoutedEventArgs e)
            => App.MainWindow.View = new InRoomView(Room);
    }
}
