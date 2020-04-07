using System;

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
    public class InRoomView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public InRoomView() => throw new InvalidOperationException();
            
        public InRoomView(Room room) {
            InitializeComponent();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}
    }
}
