using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Aleb.Client;
using Aleb.GUI.Components;
using Aleb.GUI.Popups;

namespace Aleb.GUI.Views {
    public class RoomListView: UserControl {
        IEnumerable<RoomEntry> Rooms => RoomList.Children.OfType<RoomEntry>();

        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            RoomList = this.Get<UniformGrid>("RoomList");
            Create = this.Get<Create>("Create");
        }

        UniformGrid RoomList;
        Create Create;

        public RoomListView() {
            InitializeComponent();

            Discord.Info = new DiscordRPC.RichPresence() {
                Details = "Ne radi niš"
            };
        }

        async void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            foreach (Room room in await Requests.GetRoomList())
                RoomAdded(room);

            Network.RoomAdded += RoomAdded;
            Network.RoomUpdated += RoomUpdated;
            Network.RoomDestroyed += RoomDestroyed;

            App.MainWindow.Title = "Popis soba";
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

            RoomEntry entry = new RoomEntry() { Room = room };
            entry.RoomJoined += JoinRoom;
            RoomList.Children.Add(entry);
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

            RoomList.Children.RemoveAll(Rooms.Where(i => i.Room.Name == name).ToList());
        }
        
        void CreateRoom()
            => App.MainWindow.Popup = new CreateRoomPopup();

        async void JoinRoom(Room room) {
            RoomList.IsEnabled = Create.Enabled = false;
            Focus();
            
            room = await Requests.JoinRoom(room.Name, "");

            if (room == null) {
                RoomList.IsEnabled = Create.Enabled = true;
                return;
            }

            App.MainWindow.Popup = null;
            App.MainWindow.View = new InRoomView(room);
        }
    }
}
