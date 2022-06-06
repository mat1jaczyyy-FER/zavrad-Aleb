using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Popups {
    public class ProfileSearchPopup: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            App.MainWindow.PopupTitle.Text = "Pretraži igrača";

            UserName = this.Get<ValidationTextBox>("UserName");

            SearchButton = this.Get<Button>("SearchButton");
            Status = this.Get<TextBlock>("Status");
        }

        ValidationTextBox UserName;
        
        Button SearchButton;
        TextBlock Status;

        public ProfileSearchPopup() {
            InitializeComponent();

            UserName.Validator = Validation.ValidateUsername;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void UserName_Validated(bool state) {
            if (SearchButton != null)
                SearchButton.IsEnabled = state;
        }

        void Return() => Search(null, null);

        async void Search(object sender, RoutedEventArgs e) {
            UserName.IsEnabled = App.MainWindow.PopupClose.IsEnabled = false;
            Status.Text = " ";
            Focus();
            
            Profile profile = await Requests.UserProfile(UserName.Text);

            if (profile == null) {
                UserName.IsEnabled = App.MainWindow.PopupClose.IsEnabled = true;

                Status.Text = "Igrač nije pronađen.";
                return;
            }

            App.MainWindow.Popup = null;
            App.MainWindow.Profile = new ProfileView(profile);
        }
    }
}
