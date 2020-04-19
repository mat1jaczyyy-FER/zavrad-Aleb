using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class CardStack: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Contents = this.Get<StackPanel>("Contents");
            Rotation = this.Get<LayoutTransformControl>("Root");
        }

        StackPanel Contents;
        LayoutTransformControl Rotation;

        public void ApplyPosition(int position)
            => Rotation.LayoutTransform = new RotateTransform(position % 2 == 1? 90 : 0);

        public CardStack() => throw new InvalidOperationException();

        public CardStack(List<int> cards) {
            InitializeComponent();

            foreach (int card in cards)
                Contents.Children.Add(new CardImage(card));
            
            Contents.MaxHeight = Math.Min(Contents.MaxHeight, 155 - 8 * cards.Count);
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}
    }
}