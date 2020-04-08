using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class InRoomView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            NameText = this.Get<TextBlock>("NameText");
            Users = this.Get<StackPanel>("Users").Children.OfType<UserInRoom>().ToList();

            ReadyButton = this.Get<Button>("ReadyButton");
        }

        TextBlock NameText;
        List<UserInRoom> Users;
        Button ReadyButton;

        Room _room;
        public Room Room {
            get => _room;
            set {
                _room = value;
                NameText.Text = Room.Name;
                
                for (int i = 0; i < 4; i++) {
                    Users[i].Text = Room.Users[i]?.Name?? "";
                    Users[i].Ready.State = Room.Users[i]?.Ready;
                }
            }
        }

        public InRoomView() => throw new InvalidOperationException();
            
        public InRoomView(Room room) {
            InitializeComponent();

            Room = room;
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
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UserJoined(user));
                return;
            }

            UserInRoom entry = Users.FirstOrDefault(i => i.Text == " ");
            if (entry != null) {
                entry.Text = user.Name;
                entry.Ready.State = user.Ready;
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

                if (user == Game.User)
                    ReadyButton.Content = entry.Ready.State == true? "Nisam spreman!" : "Spreman!";
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
                }
                
                Users[3].Text = "";
                Users[3].Ready.State = null;
            }
        }

        void SetReady(object sender, RoutedEventArgs e)
            => Requests.SetReady(!Users.First(i => i.Text == Game.User.Name).Ready.State?? false);

        void LeaveRoom(object sender, RoutedEventArgs e) {
            Requests.LeaveRoom();

            App.MainWindow.View = new RoomListView();
        }
    }
}
