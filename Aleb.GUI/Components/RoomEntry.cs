using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

namespace Aleb.GUI.Components {
    public class RoomEntry: IconButton {
        public delegate void RoomJoinedEventHandler(Room room);
        public event RoomJoinedEventHandler RoomJoined;

        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            NameText = this.Get<TextBlock>("NameText");
            Users = this.Get<StackPanel>("Users").Children.OfType<UserInList>().ToList();
        }

        protected override IBrush Fill {
            get => Background;
            set => Background = value;
        }

        TextBlock NameText;
        List<UserInList> Users;

        Room _room;
        public Room Room {
            get => _room;
            set {
                _room = value;
                NameText.Text = Room.Name;
                
                for (int i = 0; i < 4; i++)
                    Users[i].Text = Room.Users[i]?.Name?? "";

                Enabled = Room.Count < 4;
            }
        }

        public RoomEntry(): base("ThemeBackgroundBrush", "ThemeControlLowBrush", "ThemeControlHighBrush", "ThemeControlMidHighBrush") {
            InitializeComponent();
        }

        void JoinRoom() => RoomJoined?.Invoke(Room);
    }
}
