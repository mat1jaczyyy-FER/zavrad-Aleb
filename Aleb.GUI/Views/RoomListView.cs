using System;
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
            NoRooms = this.Get<Grid>("NoRooms");

            KickedRoot = this.Get<Grid>("KickedRoot");
            KickedText = this.Get<TextBlock>("KickedText");
        }

        UniformGrid RoomList;
        Create Create;
        Grid NoRooms;

        Grid KickedRoot;
        TextBlock KickedText;

        public RoomListView() {
            InitializeComponent();

            Discord.Info = new DiscordRPC.RichPresence() {
                Details = "Ne radi niš"
            };
        }

        async void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            foreach (Room room in await Requests.GetRoomList())
                RoomAdded(room);

            if (!Rooms.Any())
                NoRooms.IsVisible = true;

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

            NoRooms.IsVisible = false;

            RoomEntry entry = new RoomEntry() { Room = room };
            entry.RoomJoined += JoinRoom;
            entry.RoomSpectate += SpectateRoom;
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

            if (!Rooms.Any())
                NoRooms.IsVisible = true;
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

        async void SpectateRoom(Room room) {
            RoomList.IsEnabled = Create.Enabled = false;
            Focus();

            Tuple<bool, Room> response = await Requests.SpectateRoom(room.Name, "");
            bool ingame = response.Item1;
            room = response.Item2;

            if (room == null) {
                RoomList.IsEnabled = Create.Enabled = true;
                return;
            }
            
            App.MainWindow.Spectating = true;
            App.MainWindow.Popup = null;
            App.MainWindow.View = ingame? new GameView() : new InRoomView(room);
        }

        public string Kicked {
            set {
                KickedText.Text = $"Izbačeni ste iz {value}.";
                KickedRoot.IsVisible = true;
            }
        }

        void CloseKicked() => KickedRoot.IsVisible = false;
    }
}
