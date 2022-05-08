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

        bool? _state = null;
        public bool? State {
            get => _state;
            set {
                _state = value;

                Path.IsVisible = State != null;
                Resources["Brush"] = App.GetResource<IBrush>((State?? false)? "ThemeAccentBrush" : "ThemeForegroundLowBrush");
            }
        }

        public Checkmark() {
            InitializeComponent();
        }
    }
}
