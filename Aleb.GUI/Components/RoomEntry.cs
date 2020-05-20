using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;
using Aleb.GUI.Popups;

namespace Aleb.GUI.Components {
    public class RoomEntry: IconButton {
        public delegate void RoomJoinedEventHandler(Room room);
        public event RoomJoinedEventHandler RoomJoined;

        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            NameText = this.Get<TextBlock>("NameText");
            PasswordIcon = this.Get<LockIcon>("PasswordIcon");
            Settings = this.Get<TextBlock>("Settings");
            Separator = this.Get<Border>("Separator");

            Users = this.Get<StackPanel>("Users").Children.OfType<UserInList>().ToList();
        }

        protected override IBrush Fill {
            get => Background;
            set => Background = value;
        }

        TextBlock NameText, Settings;
        LockIcon PasswordIcon;
        Border Separator;
        List<UserInList> Users;

        Room _room;
        public Room Room {
            get => _room;
            set {
                _room = value;
                NameText.Text = Room.Name;
                Settings.Text = Room.Settings;

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
            if (Room.HasPassword) App.MainWindow.Popup = new PasswordEntryPopup(Room);
            else RoomJoined?.Invoke(Room);
        }
    }
}
