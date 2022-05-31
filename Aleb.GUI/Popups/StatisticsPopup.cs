using System;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Aleb.Client;

namespace Aleb.GUI.Popups {
    public class StatisticsPopup: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Keys = this.Get<StackPanel>("Keys");
            Values = this.Get<StackPanel>("Values");
        }

        StackPanel Keys, Values;

        public StatisticsPopup() => throw new InvalidOperationException();

        public StatisticsPopup(UserStats stats) {
            InitializeComponent();

            App.MainWindow.PopupTitle.Text = $"Statistike za {stats.Name}";

            foreach (Tuple<string, string> stat in UserStats.StatList) {
                Keys.Children.Add(new TextBlock() { Text = stat.Item2 });
                Values.Children.Add(new TextBlock() { Text = stats.Dict.Single(i => i.Item1 == stat.Item1).Item2.ToString() });
            }
        }
    }
}
