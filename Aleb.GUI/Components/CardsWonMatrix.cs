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
            int pack = Math.Clamp(cards.Count / 2, 4, 8);

            for (int i = 0; i < cards.Count; i++) {
                if (i % pack == 0) {
                    row = new StackPanel() {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Spacing = -30,
                        MaxHeight = 105,
                        Margin = new Thickness(rng.NextDouble() * 10, 0, 0, 0)
                    };
                    Contents.Children.Add(row);
                }
                row.Children.Add(new CardImage(cards[i]));
            }
        }
    }
}