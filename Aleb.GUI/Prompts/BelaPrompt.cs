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
    public class BelaPrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        int Index;

        public BelaPrompt() => throw new InvalidOperationException();

        public BelaPrompt(int index) {
            InitializeComponent();

            Index = index;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Skip(object sender, RoutedEventArgs e) 
            => Requests.PlayCard(Index, false);

        void Call(object sender, RoutedEventArgs e)
            => Requests.PlayCard(Index, true);
    }
}
