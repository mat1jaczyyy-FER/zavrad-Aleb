using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aleb.GUI.Components {
    public class UserInRoom: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();
            
            Ready = this.Get<Checkmark>("Ready");
        }

        public Checkmark Ready { get; private set; }

        public UserInRoom() {
            InitializeComponent();
        }
    }
}
