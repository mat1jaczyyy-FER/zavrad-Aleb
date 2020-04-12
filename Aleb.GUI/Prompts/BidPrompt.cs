using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Views;

namespace Aleb.GUI.Prompts {
    public class BidPrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Skip = this.Get<Button>("Skip");
            MustPick = this.Get<TextBlock>("MustPick");
        }

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

        void Pick(Suit? suit) => Requests.Bid(suit);

        void DontPick(object sender, RoutedEventArgs e) => Pick(null);
    }
}
