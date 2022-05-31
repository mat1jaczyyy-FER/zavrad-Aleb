using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class SpectateIcon: IconButton {
        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public SpectateIcon(): base(disabled: "ThemeButtonEnabledBrush") {
            AvaloniaXamlLoader.Load(this);

            MouseLeave(this, null);
        }
    }
}
