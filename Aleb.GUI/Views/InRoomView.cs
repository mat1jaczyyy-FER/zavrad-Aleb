using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class InRoomView: UserControl, ISpectateable {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            RoomPanel = this.Get<StackPanel>("RoomPanel");
            ResultsPanel = this.Get<StackPanel>("ResultsPanel");

            NameText = this.Get<TextBlock>("NameText");
            Settings = this.Get<TextBlock>("Settings");
            PasswordIcon = this.Get<LockIcon>("PasswordIcon");
            HiddenIcon = this.Get<HiddenIcon>("HiddenIcon");

            Users = this.Get<StackPanel>("Users").Children.OfType<UserInRoom>().ToList();
            Separator = this.Get<Border>("Separator");

            ReadyButton = this.Get<Ready>("ReadyButton");
            StartButton = this.Get<Start>("StartButton");

            ActionPanel = this.Get<StackPanel>("ActionPanel");
            
            Victory = this.Get<TextBlock>("Victory");

            Score = new TextBlock[] {
                this.Get<TextBlock>("ScoreLeft"),
                this.Get<TextBlock>("ScoreRight")
            };

            Results = this.Get<StackPanel>("ResultsLeft").Children.OfType<UserInList>()
                .Concat(this.Get<StackPanel>("ResultsRight").Children.OfType<UserInList>()).ToArray();
        }

        StackPanel RoomPanel, ResultsPanel;

        TextBlock NameText, Settings;
        LockIcon PasswordIcon;
        HiddenIcon HiddenIcon;

        List<UserInRoom> Users;
        Border Separator;

        Ready ReadyButton;
        Start StartButton;
        
        StackPanel ActionPanel;

        TextBlock Victory;
        TextBlock[] Score;
        UserInList[] Results;

        int Count = 0;

        void UpdateRoomAdmin() {
            bool isAdmin = Users[0].Text == App.User.Name;

            StartButton.IsVisible = isAdmin;
            StartButton.Enabled = isAdmin && Users.All(i => i.Ready.State == true);

            UserInRoom.AllowAdminActions = isAdmin;
        }

        string Password;

        public InRoomView() => throw new InvalidOperationException();
            
        public InRoomView(Room room) {
            InitializeComponent();

            NameText.Text = room.Name;
            Settings.Text = room.Settings;
                
            for (int i = 0; i < 4; i++) {
                Users[i].Text = room.Users[i]?.Name?? "";
                Users[i].Ready.State = room.Users[i]?.Ready;
                Users[i].Weight = room.Users[i] == App.User;
            }
            
            Count = room.Users.Count(i => i != null);
            Separator.Opacity = Convert.ToDouble(Count > 2);

            HiddenIcon.IsVisible = !room.ShowPts;
            PasswordIcon.IsVisible = room.HasPassword;
            Password = room.Password;

            UpdateRoomAdmin();
        }

        public InRoomView(List<int> score, Room room)
        : this(room) {
            ResultsPanel.IsVisible = true;
            RoomPanel.IsVisible = false;

            int rotate = Math.Max(0, room.Users.Select(i => i.Name).ToList().IndexOf(App.User.Name)) >> 1 & 1;

            string win = (score[rotate] > score[1 - rotate])? "Pobijedili" : "Izgubili";
            Victory.Text = $"{win} ste"; //todo nerijeseno

            foreach (var (text, pts) in Score.Zip(score.Rotate(rotate)))
                text.Text = pts >= Consts.BelotValue? "Belot" : pts.ToString();

            foreach (var (text, user) in Results.Zip(room.Users.Rotate(rotate * 2)))
                text.Text = user.Name;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined += UserJoined;
            Network.UserReady += UserReady;
            Network.UserLeft += UserLeft;
            Network.UsersSwitched += UsersSwitched;
            Network.Kicked += Kicked;

            Network.GameStarted += GameStarted;

            App.MainWindow.Title = NameText.Text;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined -= UserJoined;
            Network.UserReady -= UserReady;
            Network.UserLeft -= UserLeft;
            Network.UsersSwitched -= UsersSwitched;
            Network.Kicked -= Kicked;

            Network.GameStarted -= GameStarted;
        }

        async void CopyPassword()
            => await Application.Current.Clipboard.SetTextAsync(Password);

        void UserJoined(User user) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UserJoined(user));
                return;
            }

            UserInRoom entry = Users.FirstOrDefault(i => i.Text == " ");
            if (entry != null) {
                entry.Text = user.Name;
                entry.Ready.State = user.Ready;

                Separator.Opacity = Convert.ToDouble(++Count > 2);
                UpdateRoomAdmin();
            }
        }

        void UserReady(User user) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UserReady(user));
                return;
            }

            UserInRoom entry = Users.FirstOrDefault(i => i.Text == user.Name);
            if (entry != null) {
                entry.Ready.State = user.Ready;

                if (user == App.User)
                    ReadyButton.State = user.Ready;
                
                UpdateRoomAdmin();
            }
        }

        void UserLeft(User user) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UserLeft(user));
                return;
            }
            
            UserInRoom entry = Users.FirstOrDefault(i => i.Text == user.Name);
            if (entry != null) {
                for (int i = Users.IndexOf(entry); i < 3; i++) {
                    Users[i].Text = Users[i + 1].Text;
                    Users[i].Ready.State = Users[i + 1].Ready.State;
                    Users[i].Weight = Users[i + 1].Weight;
                }
                
                Users[3].Text = "";
                Users[3].Ready.State = null;
                Users[3].Weight = false;

                Separator.Opacity = Convert.ToDouble(--Count > 2);
                UpdateRoomAdmin();
            }
        }

        void UsersSwitched(User user1, User user2) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UsersSwitched(user1, user2));
                return;
            }
            
            UserInRoom entry1 = Users.FirstOrDefault(i => i.Text == user1.Name);
            UserInRoom entry2 = Users.FirstOrDefault(i => i.Text == user2.Name);

            if (entry1 != null && entry2 != null) {
                string temptext = entry1.Text;
                bool tempweight = entry1.Weight;

                entry1.Text = entry2.Text;
                entry1.Ready.State = false;
                if (user1 == App.User)
                    ReadyButton.State = false;
                entry1.Weight = entry2.Weight;

                entry2.Text = temptext;
                entry2.Ready.State = false;
                if (user2 == App.User)
                    ReadyButton.State = false;
                entry2.Weight = tempweight;

                UpdateRoomAdmin();
            }
        }

        void Kicked() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(Kicked);
                return;
            }

            App.MainWindow.View = new RoomListView(); // todo notification in corner
        }
        
        void BackToRoom(object sender, RoutedEventArgs e) {
            ResultsPanel.IsVisible = false;
            RoomPanel.IsVisible = true;
        }

        void SetReady() => Requests.SetReady(!Users.First(i => i.Text == App.User.Name).Ready.State?? false);

        void LeaveRoom() {
            Requests.LeaveRoom();

            App.MainWindow.View = new RoomListView();
        }

        void Start() => Requests.StartGame();

        void GameStarted(int dealer, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameStarted(dealer, cards));
                return;
            }

            GameView game = new GameView(Users.Select(i => i.Text).ToList()) { showPts = !HiddenIcon.IsVisible };

            App.MainWindow.View = game;
            game.GameStarted(dealer, cards);
        }

        public void Spectate() {
            ActionPanel.IsVisible = false;
            //Users.ForEach(i => i.DisableMenu());
        }
    }
}
