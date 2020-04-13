using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aleb.GUI.Components {
    public class TextOverlay: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Text = this.Get<TextBlock>("Text");
        }

        TextBlock Text;

        public TextOverlay() => throw new InvalidOperationException();

        public TextOverlay(string text, int timeout = 0) {
            InitializeComponent();

            Text.Text = text;

            if (timeout > 0)
                Task.Delay(timeout).ContinueWith(_ => IsVisible = false);
        }
    }
}