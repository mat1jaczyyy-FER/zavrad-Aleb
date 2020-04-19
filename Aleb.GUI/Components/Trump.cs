using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Aleb.Common;

namespace Aleb.GUI.Components {
    public class Trump: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Image = this.Get<Image>("Image");
            Text = this.Get<TextBlock>("Text");
        }

        Image Image;
        TextBlock Text;

        public Trump() => throw new InvalidOperationException();

        public Trump(Suit suit, string player) {
            InitializeComponent();

            Image.Source = App.GetImage($"Suits/{suit}");
            Text.Text = player;
        }
    }
}