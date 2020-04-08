using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Checkmark: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Path = this.Get<Path>("Path");
        }

        Path Path;

        public void Set(bool? value) {
            Path.IsVisible = value != null;
            Path.Stroke = (IBrush)Application.Current.Styles.FindResource(value?? false? "ThemeAccentBrush" : "ThemeForegroundLowBrush");
        }

        public Checkmark() {
            InitializeComponent();
        }
    }
}
