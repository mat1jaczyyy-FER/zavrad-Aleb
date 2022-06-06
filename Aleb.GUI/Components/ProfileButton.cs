using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

namespace Aleb.GUI.Components {
    public class ProfileButton: IconButton {
        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public ProfileButton() {
            AvaloniaXamlLoader.Load(this);

            MouseLeave(this, null);
        }

        protected async override void Click(PointerReleasedEventArgs e)
            => App.MainWindow.Profile = new ProfileView(await Requests.UserProfile(App.User.Name));
    }
}
