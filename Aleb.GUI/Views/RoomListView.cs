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
    public class RoomListView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public RoomListView() {
            InitializeComponent();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}
    }
}
