using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.GUI.Popups;

namespace Aleb.GUI.Components {
    public class PreferencesButton: IconButton {
        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public PreferencesButton() {
            AvaloniaXamlLoader.Load(this);

            MouseLeave(this, null);
        }

        protected override void Click(PointerReleasedEventArgs e)
            => App.MainWindow.Popup = new PreferencesPopup();
    }
}
