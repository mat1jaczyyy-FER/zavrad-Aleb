using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Common;

namespace Aleb.GUI.Components {
    public class TrumpButton: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Image = this.Get<Image>("Image");
        }

        public new delegate void ClickedEventHandler(Suit? suit);
        public new event ClickedEventHandler Clicked;

        Image Image;

        protected override IBrush Fill {
            get => null;
            set {}
        }

        Suit _suit;
        public Suit Suit {
            get => _suit;
            set {
                _suit = value;
                Image.Source = App.GetImage($"Suits/{value}");
            }
        }

        public TrumpButton() {
            InitializeComponent();

            MouseLeave(this, null);
        }

        protected override void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            base.Unloaded(sender, e);
            Clicked = null;
        }

        protected override void Click(PointerReleasedEventArgs e) => Clicked?.Invoke(Suit);
    }
}
