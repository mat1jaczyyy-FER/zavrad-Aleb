using Avalonia.Markup.Xaml;

namespace Aleb.GUI.Components {
    public class UserInList: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();
        }

        public UserInList() {
            InitializeComponent();
        }
    }
}
