using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
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

        bool AutoConnect = true;

        public ConnectingView() => throw new InvalidOperationException();

        public ConnectingView(bool auto) {
            InitializeComponent();

            AutoConnect = auto;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            if (AutoConnect) Connect(sender, null);
            else {
                Status.Text = "Veza s serverom izgubljena!";
                Retry.IsVisible = true;
            }
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        async void Connect(object sender, RoutedEventArgs e) {
            Status.Text = "Povezivanje...";
            Retry.IsVisible = false;

            ConnectStatus result = await Network.Connect(App.Host);

            if (result == ConnectStatus.Success) {
                Network.Disconnected += () => Dispatcher.UIThread.InvokeAsync(() => App.MainWindow.View = new ConnectingView(false));

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
