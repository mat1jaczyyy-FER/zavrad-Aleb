using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
            Type = this.Get<ComboBox>("Type");
            Goal = this.Get<ValidationTextBox>("Goal");
            ShowPts = this.Get<CheckBox>("ShowPts");
            Password = this.Get<ValidationTextBox>("Password");

            CreateButton = this.Get<Button>("CreateButton");
            Status = this.Get<TextBlock>("Status");
        }

        ValidationTextBox RoomName, Goal, Password;
        ComboBox Type;
        CheckBox ShowPts;

        Button CreateButton;
        TextBlock Status;

        // TODO REWRITE GROUPS
        bool[] Valid = new bool[3];

        int RoomGoalInt(string text)
            => int.TryParse(text, out int result)? result : 0;

        public CreateRoomPopup() {
            InitializeComponent();

            RoomName.Validator = Validation.ValidateRoomName;
            Type.SelectedItem = GameType.Prolaz;
            Goal.Validator = x => Validation.ValidateRoomGoal(RoomGoalInt(x));
            Password.Validator = Validation.ValidateRoomPassword;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Validate() {
            if (CreateButton != null)
                CreateButton.IsEnabled = Valid.All(i => i);
        }

        void RoomName_Validated(bool state) {
            Valid[0] = state;
            Validate();
        }

        void Goal_Validated(bool state) {
            Valid[1] = state;
            Validate();
        }

        void Password_Validated(bool state) {
            Valid[2] = state;
            Validate();
        }

        void Return() => Create(null, null);

        async void Create(object sender, RoutedEventArgs e) {
            RoomName.IsEnabled = Type.IsEnabled = Goal.IsEnabled = Password.IsEnabled = CreateButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = false;
            Status.Text = " ";
            Focus();
            
            Room room = await Requests.CreateRoom(RoomName.Text, (GameType)Type.SelectedItem, RoomGoalInt(Goal.Text), ShowPts.IsChecked?? false, Password.Text);

            if (room == null) {
                RoomName.IsEnabled = Type.IsEnabled = Goal.IsEnabled = Password.IsEnabled = CreateButton.IsEnabled = App.MainWindow.PopupClose.IsEnabled = true;

                Status.Text = "Nije moguće stvoriti sobu.";
                return;
            }

            room.Password = Password.Text;

            App.MainWindow.Popup = null;
            App.MainWindow.View = new InRoomView(room);
        }
    }
}
