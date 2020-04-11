using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;

namespace Aleb.GUI.Views {
    public class GameView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            UserText = this.Get<DockPanel>("Root").Children.OfType<UserInRoom>().ToList();
            Cards = this.Get<StackPanel>("Cards");
        }

        List<UserInRoom> UserText;
        StackPanel Cards;

        public GameView() => throw new InvalidOperationException();

        public GameView(List<string> names) {
            InitializeComponent();
            
            UserText.Swap(1, 2);
            names.Swap(1, 2);

            foreach (var (text, name) in UserText.Zip(names.RotateWith(i => i == App.User.Name)))
                text.Text = name;

            UserText = UserText.RotateWith(i => i.Text == names[0]).ToList();
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted += GameStarted;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted -= GameStarted;
        }

        public void GameStarted(int dealer, List<int> cards) {
            for (int i = 0; i < 4; i++)
                UserText[i].Ready.State = i == dealer;

            Cards.Children.Clear();

            foreach (int card in cards)
                Cards.Children.Add(new CardImage(card));

            Cards.Children.Add(new CardImage(32));
            Cards.Children.Add(new CardImage(32));
        }
    }
}
