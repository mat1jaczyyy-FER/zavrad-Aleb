using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;
using Aleb.GUI.Popups;

namespace Aleb.GUI.Components {
    public class UserInRoom: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();
            
            Ready = this.Get<Checkmark>("Ready");
            Menu = (AlebContextMenu)this.Resources["Menu"];
        }

        public Checkmark Ready { get; private set; }

        public bool Weight {
            get => User.FontWeight == FontWeight.Bold;
            set => User.FontWeight = value? FontWeight.Bold : FontWeight.Regular;
        }

        AlebContextMenu Menu;

        public UserInRoom() {
            InitializeComponent();
            
            this.AddHandler(DragDrop.DragOverEvent, DragOver);
            this.AddHandler(DragDrop.DropEvent, Drop);
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            this.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            this.RemoveHandler(DragDrop.DropEvent, Drop);
        }

        const string DragFormat = "aleb-user";

        public static bool AllowAdminActions;
        public bool CanDragDrop => AllowAdminActions && Text != " ";

        bool CanDrag(PointerPressedEventArgs e)
            => CanDragDrop && e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed;

        bool CanDrop(DragEventArgs e)
            => CanDragDrop && e.Data.GetDataFormats().Contains(DragFormat);

        async void Drag(object sender, PointerPressedEventArgs e) {
            if (!CanDrag(e)) return;

            DataObject dragData = new DataObject();
            dragData.Set(DragFormat, this);

            App.Dragging = true;
            await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            App.Dragging = false;
        }

        void DragOver(object sender, DragEventArgs e) {
            e.DragEffects = CanDrop(e)? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        void Drop(object sender, DragEventArgs e) {
            if (!CanDrop(e)) return;

            if (e.Data.Get(DragFormat) is UserInRoom dragged) {
                if (dragged == this || !dragged.CanDragDrop) return;
                
                Requests.SwitchUsers(Text, dragged.Text);
                e.Handled = true;
            }
        }

        void MenuOpen(object sender, PointerReleasedEventArgs e) {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonReleased) return;

            e.Handled = true;

            Menu?.Open(
                this,
                AllowAdminActions && App.User.Name != Text
                    ? null
                    : new string[] { "Izbaci" }
            );
        }

        async void MenuAction(string action) {
            if (action == "Statistike")
                App.MainWindow.Popup = new StatisticsPopup(await Requests.UserStats(Text));
            
            if (action == "Izbaci" && AllowAdminActions)
                Requests.KickUser(Text);
        }

        public void DisableMenu() => Menu = null;
    }
}
