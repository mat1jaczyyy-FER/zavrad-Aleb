using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;

namespace Aleb.GUI.Views {
    public class ConnectingView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Status = this.Get<TextBlock>("Status");
            Retry = this.Get<Button>("Retry");
        }

        TextBlock Status;
        Button Retry;

        public ConnectingView() {
            InitializeComponent();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) => Connect(sender, null);

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        async void Connect(object sender, RoutedEventArgs e) {
            Status.Text = "Povezivanje...";
            Retry.IsVisible = false;

            ConnectStatus result = await Network.Connect(App.Host);

            if (result == ConnectStatus.Success) {
                App.MainWindow.View = new LoginView();

            } else if (result == ConnectStatus.VersionMismatch)
                Status.Text = "Verzija klijenta ne podudara verziji servera.";

            else if (result == ConnectStatus.Failed) {
                Status.Text = "Povezivanje neuspjelo!";
                Retry.IsVisible = true;
            }
        }
    }
}
