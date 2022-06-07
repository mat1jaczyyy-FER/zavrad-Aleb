using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;

namespace Aleb.GUI.Prompts {
    public class BelaPrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            SkipButton = this.Get<Button>("SkipButton");
            CallButton = this.Get<Button>("CallButton");
        }

        Button SkipButton, CallButton;

        public BelaPrompt() {
            InitializeComponent();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Skip(object sender, RoutedEventArgs e) {
            SkipButton.IsEnabled = CallButton.IsEnabled = false;
            Requests.Bela(false);
        }

        void Call(object sender, RoutedEventArgs e) {
            SkipButton.IsEnabled = CallButton.IsEnabled = false;
            Requests.Bela(true);
        }
    }
}
