using Avalonia.Controls;

namespace Aleb.GUI.Components {
    public abstract class UserDisplay: UserControl {
        protected virtual void InitializeComponent() {
            User = this.Get<TextBlock>("User");
        }

        protected TextBlock User;

        public string Text {
            get => User.Text;
            set {
                if (value.Length > 0 && value != " ") {
                    User.Text = value;
                    Opacity = 1;

                } else {
                    User.Text = " ";
                    Opacity = 0;
                }
            }
        }
    }
}
