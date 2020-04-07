using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Maximize: IconButton {
        public new delegate void ClickedEventHandler(PointerEventArgs e);
        public new event ClickedEventHandler Clicked;

        protected override IBrush Fill {
            get => (IBrush)Resources["Brush"];
            set => Resources["Brush"] = value;
        }

        public Maximize() {
            AvaloniaXamlLoader.Load(this);

            MouseLeave(this, null);
        }

        protected override void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            base.Unloaded(sender, e);
            Clicked = null;
        }

        protected override void Click(PointerReleasedEventArgs e) => Clicked?.Invoke(e);
    }
}
