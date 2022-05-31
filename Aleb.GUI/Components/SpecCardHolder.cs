using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class SpecCardHolder: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Cards = this.Get<StackPanel>("Cards");
            Rotation = this.Get<LayoutTransformControl>("Root");
        }

        StackPanel Cards;
        LayoutTransformControl Rotation;

        public bool Vertical {
            set {
                Rotation.LayoutTransform = value ? new RotateTransform(90) : new RotateTransform(0);
            }
        }

        List<int> lastCards = null;

        void SetStack(CardStack stack) {
            Cards.Children.Clear();

            Cards.Children.Add(stack);
        }

        public void SetCards(List<int> cards) {
            Cards.Children.Clear();

            if (cards != null && cards.Count > 0) {
                SetStack(new CardStack(cards.Take(6).ToList(), cards.Skip(6).ToList()));
                Opacity = 1;

            } else Opacity = 0;
            
            lastCards = cards;
        }
        
        public void RevealTalon() {
            if (lastCards == null) return;

            lastCards.Sort();

            SetStack(new CardStack(lastCards));
        }

        public void CardPlayed(int card) {
            if (lastCards == null) return;

            lastCards = lastCards.Where(i => i != card).ToList();

            if (lastCards.Count > 0)
                SetStack(new CardStack(lastCards));
            
            else
                SetCards(null);
        }

        public SpecCardHolder()
        : this(null) {}

        public SpecCardHolder(List<int> cards) {
            InitializeComponent();

            SetCards(cards);
        }
    }
}