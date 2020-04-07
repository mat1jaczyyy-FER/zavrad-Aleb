using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.GUI.Components;
using Aleb.GUI.Popups;

namespace Aleb.GUI.Views {
    public class RoomListView: UserControl {
        IEnumerable<RoomEntry> Rooms => RoomList.Children.OfType<RoomEntry>();

        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            RoomList = this.Get<UniformGrid>("RoomList");
        }

        UniformGrid RoomList;

        public RoomListView() {
            InitializeComponent();
        }

        async void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            foreach (Room room in await Requests.GetRoomList())
                RoomAdded(room);

            Network.RoomAdded += RoomAdded;
            Network.RoomUpdated += RoomUpdated;
            Network.RoomDestroyed += RoomDestroyed;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.RoomAdded -= RoomAdded;
            Network.RoomUpdated -= RoomUpdated;
            Network.RoomDestroyed -= RoomDestroyed;
        }

        void RoomAdded(Room room) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => RoomAdded(room));
                return;
            }

            RoomList.Children.Add(new RoomEntry() { Room = room });
        }

        void RoomUpdated(Room room) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => RoomUpdated(room));
                return;
            }

            RoomEntry entry = Rooms.FirstOrDefault(i => i.Room == room);
            if (entry != null) entry.Room = room;
        }

        void RoomDestroyed(string name) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => RoomDestroyed(name));
                return;
            }

            RoomList.Children.RemoveAll(Rooms.Where(i => i.Room.Name == name));
        }
        
        void CreateRoom() {
            App.MainWindow.Popup = new CreateRoomPopup();
            App.MainWindow.PopupTitle.Text = "Stvori sobu";
        }
    }
}
