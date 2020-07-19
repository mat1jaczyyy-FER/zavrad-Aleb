using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Aleb.GUI.Components {
    public class TextOverlay: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Root = this.Get<StackPanel>("Root");
        }

        StackPanel Root;

        public void SetControl(Control control, int timeout = 0) {
            Root.Children.Clear();

            if (control != null) {
                control.Margin = Thickness.Parse("40 20");

                Root.Children.Add(control);
                IsHitTestVisible = true;
                Opacity = 1;

                if (timeout > 0)
                    Task.Delay(timeout).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => IsVisible = false));

            } else {
                IsHitTestVisible = false;
                Opacity = 0;
            }
        }

        public TextOverlay()
        : this((Control)null) {}

        public TextOverlay(string text, int timeout = 0)
        : this(new TextBlock() { Text = text }, timeout) {}

        public TextOverlay(Control control, int timeout = 0) {
            InitializeComponent();
            
            SetControl(control, timeout);
        }
    }
}