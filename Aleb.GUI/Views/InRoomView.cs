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
                    Users[i].Ready.Set(Room.Users[i]?.Ready);
                }
            }
        }

        public InRoomView() => throw new InvalidOperationException();
            
        public InRoomView(Room room) {
            InitializeComponent();

            Room = room;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void SetReady(object sender, RoutedEventArgs e) {}

        void LeaveRoom(object sender, RoutedEventArgs e) {}
    }
}
