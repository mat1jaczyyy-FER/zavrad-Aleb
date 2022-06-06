using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;
using Aleb.GUI.Popups;

namespace Aleb.GUI.Components {
    public class ProfileButton: IconButton {
        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public ProfileButton() {
            AvaloniaXamlLoader.Load(this);

            AllowRightClick = true;

            MouseLeave(this, null);
        }

        protected async override void Click(PointerReleasedEventArgs e) {
            PointerUpdateKind MouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

            if (MouseButton == PointerUpdateKind.LeftButtonReleased)
                App.MainWindow.Profile = new ProfileView(await Requests.UserProfile(App.User.Name));
            
            if (MouseButton == PointerUpdateKind.RightButtonReleased)
                App.MainWindow.Popup = new ProfileSearchPopup();
        }
    }
}
