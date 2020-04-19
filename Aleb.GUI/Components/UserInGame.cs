using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class UserInGame: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();

            DealerIcon = this.Get<DealerIcon>("DealerIcon");

            Rotation = this.Get<LayoutTransformControl>("Root");
            Border = this.Get<Border>("Border");
        }

        LayoutTransformControl Rotation;
        Border Border;

        public bool Playing {
            set {
                Background = value
                    ? (IBrush)Application.Current.FindResource("ThemeControlLowBrush")
                    : new SolidColorBrush(Color.Parse("Transparent"));

                Border.BorderBrush = value
                    ? (IBrush)Application.Current.FindResource("ThemeAccentBrush")
                    : new SolidColorBrush(Color.Parse("Transparent"));
            }
        }

        public bool VerticalText {
            set => Rotation.LayoutTransform = new RotateTransform(value? 90 : 0);
        }

        public DealerIcon DealerIcon { get; private set; }

        public UserInGame() {
            InitializeComponent();
        }
    }
}
