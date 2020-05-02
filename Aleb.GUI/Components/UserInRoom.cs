using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class UserInRoom: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();
            
            Ready = this.Get<Checkmark>("Ready");
        }

        public Checkmark Ready { get; private set; }

        public bool Weight {
            get => User.FontWeight == FontWeight.Bold;
            set => User.FontWeight = value? FontWeight.Bold : FontWeight.Regular;
        }

        public UserInRoom() {
            InitializeComponent();
        }
    }
}
