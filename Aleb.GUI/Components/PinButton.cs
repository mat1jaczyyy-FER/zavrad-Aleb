using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class PinButton: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            PathUnpinned = this.Get<Path>("PathUnpinned");
            PathPinned = this.Get<Path>("PathPinned");
        }

        Path PathUnpinned, PathPinned;
        Path GetPath(bool current = true) => (current == Preferences.Topmost)? PathPinned : PathUnpinned;

        protected override IBrush Fill {
            get => GetPath().Stroke;
            set => PathUnpinned.Fill = PathUnpinned.Stroke = PathPinned.Fill = PathPinned.Stroke = value;
        }

        public PinButton() {
            InitializeComponent();

            MouseLeave(this, null);

            GetPath().Opacity = 1;
        }
        
        protected override void Click(PointerReleasedEventArgs e) {
            Preferences.Topmost = !Preferences.Topmost;
            
            GetPath(false).Opacity = 0;
            GetPath().Opacity = 1;
        }
    }
}
