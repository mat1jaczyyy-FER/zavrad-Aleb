using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;
using Aleb.GUI.Popups;
using Aleb.GUI.Views;

namespace Aleb.GUI.Components {
    public class RoomEntry: IconButton {
        public delegate void RoomEventHandler(Room room);
        public event RoomEventHandler RoomJoined, RoomSpectate;

        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            NameText = this.Get<TextBlock>("NameText");
            PasswordIcon = this.Get<LockIcon>("PasswordIcon");
            HiddenIcon = this.Get<HiddenIcon>("HiddenIcon");
            Settings = this.Get<TextBlock>("Settings");
            Separator = this.Get<Border>("Separator");

            SpectateIcon = this.Get<SpectateIcon>("SpectateIcon");

            Users = this.Get<StackPanel>("Users").Children.OfType<UserInList>().ToList();
        }

        protected override IBrush Fill {
            get => Background;
            set => Background = value;
        }

        TextBlock NameText, Settings;
        LockIcon PasswordIcon;
        HiddenIcon HiddenIcon;
        Border Separator;
        List<UserInList> Users;
        SpectateIcon SpectateIcon;

        Room _room;
        public Room Room {
            get => _room;
            set {
                _room = value;
                NameText.Text = Room.Name;
                Settings.Text = Room.Settings;

                HiddenIcon.IsVisible = !Room.ShowPts;
                PasswordIcon.IsVisible = Room.HasPassword;
                
                for (int i = 0; i < 4; i++)
                    Users[i].Text = Room.Users[i]?.Name?? "";

                Separator.Opacity = Convert.ToDouble(Room.Count > 2);

                Enabled = Room.Count < 4;
            }
        }

        public RoomEntry(): base("ThemeBackgroundBrush", "ThemeControlLowBrush", "ThemeControlHighBrush", "ThemeControlMidHighBrush")
            => InitializeComponent();

        void JoinRoom() {
            if (Room.HasPassword) {
                App.MainWindow.Popup = new PasswordEntryPopup(
                    Room,
                    async (room, password) => new Tuple<bool, Room>(false, await Requests.JoinRoom(room.Name, password)),
                    (ingame, room) => {
                        App.MainWindow.Popup = null;
                        App.MainWindow.View = new InRoomView(room);
                    }
                );
                
            } else RoomJoined?.Invoke(Room);
        }

        void SpectateRoom() {
            if (Room.HasPassword) {
                App.MainWindow.Popup = new PasswordEntryPopup(
                    Room,
                    (room, password) => Requests.SpectateRoom(room.Name, password),
                    (ingame, room) => {
                        App.MainWindow.Spectating = true;
                        App.MainWindow.Popup = null;
                        App.MainWindow.View = ingame? new GameView() : new InRoomView(room);
                    }
                );

            } else RoomSpectate?.Invoke(Room);
        }
    }
}
