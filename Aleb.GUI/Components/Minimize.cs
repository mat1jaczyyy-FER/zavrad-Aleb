using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Minimize: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Path = this.Get<Path>("Path");
        }

        Path Path;

        protected override IBrush Fill {
            get => Path.Stroke;
            set => Path.Stroke = value;
        }

        public Minimize() {
            InitializeComponent();

            MouseLeave(this, null);
        }
    }
}
