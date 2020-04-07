using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class RoomListView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            RoomList = this.Get<UniformGrid>("RoomList");
        }

        UniformGrid RoomList;

        public RoomListView() {
            InitializeComponent();
        }

        async void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Game.Rooms = await Requests.GetRoomList();

            foreach (Room room in Game.Rooms)
                RoomList.Children.Add(new RoomEntry() { Room = room });
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}
    }
}
