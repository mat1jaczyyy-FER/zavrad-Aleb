using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.Client;

namespace Aleb.GUI.Prompts {
    public class DeclarePrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            SkipButton = this.Get<Button>("SkipButton");
            CallButton = this.Get<Button>("CallButton");
        }
        
        Button SkipButton, CallButton;

        bool[] Declared;

        public DeclarePrompt() => throw new InvalidOperationException();

        public DeclarePrompt(bool[] declared) {
            InitializeComponent();

            Declared = declared;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Skip(object sender, RoutedEventArgs e) {
            SkipButton.IsEnabled = CallButton.IsEnabled = false;

            Requests.Declare(null);
        }

        void Call(object sender, RoutedEventArgs e) {
            SkipButton.IsEnabled = CallButton.IsEnabled = false;
            
            Requests.Declare(Declared.Select((x, i) => (x, i)).Where(t => t.x).Select(t => t.i).ToList());
        }
    }
}
