using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class DealerIcon: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Ellipse = this.Get<Ellipse>("Ellipse");
        }

        Ellipse Ellipse;

        bool _state = false;
        public bool State {
            get => _state;
            set {
                _state = value;

                Ellipse.Opacity = Convert.ToInt32(State);
            }
        }

        public DealerIcon() {
            InitializeComponent();
        }
    }
}
