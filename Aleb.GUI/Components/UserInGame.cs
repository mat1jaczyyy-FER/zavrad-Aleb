using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

namespace Aleb.GUI.Components {
    public class UserInGame: UserDisplay {
        protected override void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            base.InitializeComponent();

            DealerIcon = this.Get<DealerIcon>("DealerIcon");
        }

        public bool Playing {
            set {
                Background = value
                    ? (IBrush)Application.Current.FindResource("ThemeForegroundLowBrush")
                    : new SolidColorBrush(Color.Parse("Transparent"));
            }
        }

        public DealerIcon DealerIcon { get; private set; }

        public UserInGame() {
            InitializeComponent();
        }
    }
}
