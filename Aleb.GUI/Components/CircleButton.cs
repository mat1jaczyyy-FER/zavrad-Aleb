using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public abstract class CircleButton: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Icon = this.Get<Canvas>("Icon");
            Ellipse = this.Get<Ellipse>("Ellipse");
        }

        Canvas Icon;
        Ellipse Ellipse;

        protected override IBrush Fill {
            get => Ellipse.Fill;
            set => Ellipse.Fill = value;
        }

        bool ShouldFill;

        public CircleButton() => throw new InvalidOperationException();

        public CircleButton(List<Path> illustration, bool fill = false)
        : base("ThemeControlHighBrush", "ThemeControlLowBrush", "ThemeControlHighestBrush", "ThemeControlVeryHighBrush") {
            InitializeComponent();

            MouseLeave(this, null);
            
            ShouldFill = fill;
            SetIllustration(illustration);
        }

        public CircleButton(Path path, bool fill = false)
        : this(new List<Path> { path }, fill) {}

        protected void SetIllustration(List<Path> paths) {
            Icon.Children.RemoveAll(Icon.Children.OfType<Path>().ToArray());
                
            foreach (Path value in paths) {
                value.IsHitTestVisible = false;
                value.Stroke = (IBrush)Application.Current.Styles.FindResource("ThemeForegroundBrush");
                if (ShouldFill) value.Fill = value.Stroke;

                Icon.Children.Add(value);
            }
        }

        protected void SetIllustration(Path path)
            => SetIllustration(new List<Path> { path });
    }
}
