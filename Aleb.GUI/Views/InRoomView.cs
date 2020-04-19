using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Aleb.Client;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class InRoomView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            NameText = this.Get<TextBlock>("NameText");
            Settings = this.Get<TextBlock>("Settings");
            Users = this.Get<StackPanel>("Users").Children.OfType<UserInRoom>().ToList();

            ReadyButton = this.Get<Button>("ReadyButton");
            StartButton = this.Get<Button>("StartButton");
        }

        TextBlock NameText, Settings;
        List<UserInRoom> Users;
        Button ReadyButton, StartButton;

        void EnableStartButton() {
            StartButton.IsVisible = Users[0].Text == App.User.Name;
            StartButton.IsEnabled = StartButton.IsVisible && Users.All(i => i.Ready.State == true);
        }

        public InRoomView() => throw new InvalidOperationException();
            
        public InRoomView(Room room) {
            InitializeComponent();

            NameText.Text = room.Name;
            Settings.Text = room.Settings;
                
            for (int i = 0; i < 4; i++) {
                Users[i].Text = room.Users[i]?.Name?? "";
                Users[i].Ready.State = room.Users[i]?.Ready;
            }

            EnableStartButton();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined += UserJoined;
            Network.UserReady += UserReady;
            Network.UserLeft += UserLeft;
            Network.GameStarted += GameStarted;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.UserJoined -= UserJoined;
            Network.UserReady -= UserReady;
            Network.UserLeft -= UserLeft;
            Network.GameStarted -= GameStarted;
        }

        void UserJoined(User user) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UserJoined(user));
                return;
            }

            UserInRoom entry = Users.FirstOrDefault(i => i.Text == " ");
            if (entry != null) {
                entry.Text = user.Name;
                entry.Ready.State = user.Ready;
            }
            
            EnableStartButton();
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
                    ReadyButton.Content = entry.Ready.State == true? "Nisam spreman!" : "Spreman!";
            }
            
            EnableStartButton();
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
                }
                
                Users[3].Text = "";
                Users[3].Ready.State = null;
            }
            
            EnableStartButton();
        }

        void SetReady(object sender, RoutedEventArgs e)
            => Requests.SetReady(!Users.First(i => i.Text == App.User.Name).Ready.State?? false);

        void LeaveRoom(object sender, RoutedEventArgs e) {
            Requests.LeaveRoom();

            App.MainWindow.View = new RoomListView();
        }

        void Start(object sender, RoutedEventArgs e) 
            => Requests.StartGame();

        void GameStarted(int dealer, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameStarted(dealer, cards));
                return;
            }

            App.MainWindow.View = new GameView(Users.Select(i => i.Text).ToList(), dealer, cards);
        }
    }
}
