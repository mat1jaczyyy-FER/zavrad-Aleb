using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Prompts {
    public class BidPrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            PickButtons = this.Get<StackPanel>("PickRoot").Children.OfType<TrumpButton>().ToArray();
            Skip = this.Get<Button>("Skip");
            MustPick = this.Get<TextBlock>("MustPick");
        }

        TrumpButton[] PickButtons;
        Button Skip;
        TextBlock MustPick;

        public BidPrompt() => throw new InvalidOperationException();

        public BidPrompt(bool last) {
            InitializeComponent();

            if (last) {
                Skip.IsVisible = false;
                MustPick.IsVisible = true;
            }
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Pick(Suit? suit) {
            foreach (TrumpButton button in PickButtons)
                button.IsEnabled = false;
            
            Skip.IsEnabled = false;

            Requests.Bid(suit);
        }

        void DontPick(object sender, RoutedEventArgs e) => Pick(null);
    }
}
