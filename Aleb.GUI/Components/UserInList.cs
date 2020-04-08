using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Client;

namespace Aleb.GUI.Components {
    public class UserInList: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            User = this.Get<TextBlock>("User");
        }

        TextBlock User;

        public string Text {
            get => User.Text;
            set {
                if (value.Length > 0) {
                    User.Text = value;
                    Opacity = 1;

                } else {
                    User.Text = " ";
                    Opacity = 0;
                }
            }
        }

        public UserInList() {
            InitializeComponent();
        }
    }
}
