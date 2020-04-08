using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

namespace Aleb.GUI.Components {
    public class UserInRoom: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            User = this.Get<TextBlock>("User");
            Ready = this.Get<Checkmark>("Ready");
        }

        TextBlock User;

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

        public Checkmark Ready;

        public UserInRoom() {
            InitializeComponent();
        }
    }
}
