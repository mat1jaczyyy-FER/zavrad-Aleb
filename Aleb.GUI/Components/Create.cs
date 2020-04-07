using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Create: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Ellipse = this.Get<Ellipse>("Ellipse");
        }

        Ellipse Ellipse;

        protected override IBrush Fill {
            get => Ellipse.Fill;
            set => Ellipse.Fill = value;
        }

        public Create(): base("ThemeControlMidHighBrush", "ThemeControlLowBrush", "ThemeControlVeryHighBrush", "ThemeControlHighBrush") {
            InitializeComponent();

            MouseLeave(this, null);
        }
    }
}
