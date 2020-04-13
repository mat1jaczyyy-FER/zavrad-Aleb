using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

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
