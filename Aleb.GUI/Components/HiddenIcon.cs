using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class HiddenIcon: IconButton {
        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public HiddenIcon(): base(disabled: "ThemeButtonEnabledBrush") {
            AvaloniaXamlLoader.Load(this);

            Enabled = false;

            MouseLeave(this, null);
        }
    }
}
