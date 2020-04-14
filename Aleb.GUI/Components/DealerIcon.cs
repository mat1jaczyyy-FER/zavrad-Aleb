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
        }

        public DealerIcon() {
            InitializeComponent();
        }
    }
}
