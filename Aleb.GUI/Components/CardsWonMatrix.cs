using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace Aleb.GUI.Components {
    public class CardsWonMatrix: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Contents = this.Get<StackPanel>("Contents");
        }

        StackPanel Contents;
        static Random rng = new Random();

        public CardsWonMatrix() => throw new InvalidOperationException();

        public CardsWonMatrix(List<int> cards) {
            InitializeComponent();

            StackPanel row = null;

            for (int i = 0; i < cards.Count; i++) {
                if (i % 8 == 0) {
                    row = new StackPanel() {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Spacing = -30,
                        MaxHeight = 105,
                        Margin = new Thickness(rng.NextDouble() * 10, 0, 0, 0)
                    };
                    Contents.Children.Add(row);
                }
                if (i % 8 == 4) {
                    CardImage empty = new CardImage(cards[i]);
                    empty.Opacity = 0;
                    row.Children.Add(empty);
                }
                row.Children.Add(new CardImage(cards[i]));
            }
        }
    }
}