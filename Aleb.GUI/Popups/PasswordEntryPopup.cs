using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Popups {
    public class PasswordEntryPopup: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            App.MainWindow.PopupTitle.Text = "Unesi lozinku";

            Password = this.Get<ValidationTextBox>("Password");

            JoinButton = this.Get<Button>("JoinButton");
            Status = this.Get<TextBlock>("Status");
        }

        ValidationTextBox Password;

        Button JoinButton;
        TextBlock Status;

        Room Room;
        Func<Room, string, Task<Tuple<bool, Room>>> Action;
        Action<bool, Room> Transition;

        public PasswordEntryPopup() => throw new InvalidOperationException();

        public PasswordEntryPopup(Room room, Func<Room, string, Task<Tuple<bool, Room>>> action, Action<bool, Room> transition) {
            InitializeComponent();

            Room = room;
            Action = action;
            Transition = transition;

            Password.Validator = Validation.ValidateRoomPassword;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Password_Validated(bool state) {
            if (JoinButton != null)
                JoinButton.IsEnabled = state;
        }

        void Return() => Join(null, null);

        async void Join(object sender, RoutedEventArgs e) {
            Password.IsEnabled = JoinButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = false;
            Status.Text = " ";
            Focus();
            
            Tuple<bool, Room> response = await Action(Room, Password.Text);
            bool ingame = response.Item1;
            Room room = response.Item2;

            if (response.Item2 == null) {
                Password.IsEnabled = JoinButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = true;

                Status.Text = "Netočna lozinka.";
                return;
            }

            room.Password = Password.Text;

            Transition(ingame, room);
        }
    }
}
