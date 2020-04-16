using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Views;

namespace Aleb.GUI.Popups {
    public class CreateRoomPopup: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            App.MainWindow.PopupTitle.Text = "Stvori sobu";

            RoomName = this.Get<ValidationTextBox>("RoomName");
            CreateButton = this.Get<Button>("CreateButton");
            Status = this.Get<TextBlock>("Status");
        }

        ValidationTextBox RoomName;
        Button CreateButton;
        TextBlock Status;

        public CreateRoomPopup() {
            InitializeComponent();

            RoomName.Validator = Validation.ValidateRoomName;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void RoomName_Validated(bool state) => CreateButton.IsEnabled = state;

        void Return() => Create(null, null);

        async void Create(object sender, RoutedEventArgs e) {
            RoomName.IsEnabled = CreateButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = false;
            Status.Text = " ";
            Focus();
            
            Room room = await Requests.CreateRoom(RoomName.Text);

            if (room == null) {
                RoomName.IsEnabled = true;
                CreateButton.IsEnabled = false;
                App.MainWindow.PopupClose.IsEnabled = true;

                Status.Text = "Nije moguće stvoriti sobu.";
                return;
            }

            App.MainWindow.Popup = null;
            App.MainWindow.View = new InRoomView(room);
        }
    }
}
