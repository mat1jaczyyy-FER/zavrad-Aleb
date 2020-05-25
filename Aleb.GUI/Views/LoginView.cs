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
    public class LoginView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Username = this.Get<ValidationTextBox>("Username");
            Password = this.Get<ValidationTextBox>("Password");
            LoginButton = this.Get<Button>("LoginButton");
            Status = this.Get<TextBlock>("Status");
        }

        ValidationTextBox Username, Password;
        Button LoginButton;
        TextBlock Status;

        bool[] Valid = new bool[2];

        public LoginView() {
            InitializeComponent();

            Username.Validator = Validation.ValidateUsername;
            Password.Validator = Validation.ValidatePassword;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e)
            => App.MainWindow.Title = "Prijava";

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Validate() {
            if (LoginButton != null)
                LoginButton.IsEnabled = Valid.All(i => i);
        }

        void Username_Validated(bool state) {
            Valid[0] = state;
            Validate();
        }

        void Password_Validated(bool state) {
            Valid[1] = state;
            Validate();
        }

        void Return() => Login(null, null);

        async void Login(object sender, RoutedEventArgs e) {
            Username.IsEnabled = Password.IsEnabled = LoginButton.IsEnabled = false;
            Status.Text = "";
            Focus();
            
            App.User = await Requests.Login(Username.Text, Password.Text);

            if (App.User == null) {
                Username.IsEnabled = Password.IsEnabled = true;
                LoginButton.IsEnabled = false;

                Status.Text = "Prijava nije uspjela!";
                return;
            }

            if (App.User.State == UserState.Idle)
                App.MainWindow.View = new RoomListView();

            if (App.User.State == UserState.InGame)
                App.MainWindow.View = new GameView();
        }
    }
}
