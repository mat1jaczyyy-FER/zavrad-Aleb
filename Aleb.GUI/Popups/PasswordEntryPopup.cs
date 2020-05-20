using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Views;
using System;

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

        public PasswordEntryPopup() => throw new InvalidOperationException();

        public PasswordEntryPopup(Room room) {
            InitializeComponent();

            Room = room;

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
            
            Room room = await Requests.JoinRoom(Room.Name, Password.Text);

            if (room == null) {
                Password.IsEnabled = JoinButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = true;

                Status.Text = "Netočna lozinka.";
                return;
            }

            room.Password = Password.Text;

            App.MainWindow.Popup = null;
            App.MainWindow.View = new InRoomView(room);
        }
    }
}
