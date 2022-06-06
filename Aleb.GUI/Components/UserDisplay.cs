using Avalonia.Controls;
using Avalonia.Input;

namespace Aleb.GUI.Components {
    public abstract class UserDisplay: UserControl {
        protected virtual void InitializeComponent() {
            User = this.Get<TextBlock>("User");
            
            if (this.Resources.TryGetValue("Menu", out object value))
                Menu = (AlebContextMenu)value;
        }

        protected AlebContextMenu Menu;
        protected TextBlock User;

        public string Text {
            get => User.Text;
            set => User.Text = (value.Length > 0 && value != " ")? value : " ";
        }

        protected void MenuOpen(object sender, PointerReleasedEventArgs e) {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonReleased) return;

            e.Handled = true;

            Menu?.Open(this, MenuHides());
        }
        
        protected virtual string[] MenuHides() => null;
    }
}
