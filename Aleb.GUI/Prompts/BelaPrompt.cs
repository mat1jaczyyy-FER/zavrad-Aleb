using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;

namespace Aleb.GUI.Prompts {
    public class BelaPrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public BelaPrompt() {
            InitializeComponent();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Skip(object sender, RoutedEventArgs e) 
            => Requests.Bela(false);

        void Call(object sender, RoutedEventArgs e)
            => Requests.Bela(true);
    }
}
