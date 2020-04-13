using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class CardImage: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Image = this.Get<Image>("Image");
        }

        public new delegate void ClickedEventHandler(CardImage sender);
        public new event ClickedEventHandler Clicked;

        Image Image;

        protected override IBrush Fill {
            get => null;
            set { }
        }

        public CardImage() => throw new InvalidOperationException();

        public CardImage(int card) {
            InitializeComponent();

            Image.Source = App.GetImage($"Cards/{card}");

            MouseLeave(this, null);
        }

        protected override void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            base.Unloaded(sender, e);
            Clicked = null;
        }

        protected override void Click(PointerReleasedEventArgs e) => Clicked?.Invoke(this);
    }
}