using System;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Common;
using Aleb.Client;

namespace Aleb.GUI.Components {
    public class ProfileView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            User = this.Get<TextBlock>("User");
            StatisticsText = this.Get<TextBlock>("StatisticsText");
            HistoryText = this.Get<TextBlock>("HistoryText");

            StatisticsSelected = this.Get<Border>("StatisticsSelected");
            HistorySelected = this.Get<Border>("HistorySelected");

            StatisticsRoot = this.Get<Grid>("StatisticsRoot");
            Keys = this.Get<StackPanel>("Keys");
            Values = this.Get<StackPanel>("Values");

            HistoryRoot = this.Get<StackPanel>("HistoryRoot");
        }

        TextBlock User, StatisticsText, HistoryText;
        Border StatisticsSelected, HistorySelected;
        
        Grid StatisticsRoot;
        StackPanel Keys, Values;

        StackPanel HistoryRoot;

        public ProfileView() => throw new InvalidOperationException();

        public ProfileView(Profile profile) {
            InitializeComponent();

            User.Text = profile.Name;

            foreach (Tuple<string, string> stat in Profile.StatList) {
                Keys.Children.Add(new TextBlock() { Text = stat.Item2 });
                Values.Children.Add(new TextBlock() { Text = profile.Statistics.Single(i => i.Item1 == stat.Item1).Item2.ToString() });
            }

            if (profile.MatchHistory.Any())
                HistoryRoot.Children.Clear();

            for (int i = 0; i < profile.MatchHistory.Count; i++)
                HistoryRoot.Children.Add(
                    new MatchMetadata(
                        profile.MatchHistory[profile.MatchHistory.Count - i - 1],
                        profile.Name
                    ) { AlternatingColor = i % 2 == 1 }
                );
        }

        void StatisticsClick(object sender, PointerReleasedEventArgs e) {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased) return;
            
            e.Handled = true;

            HistoryText.Foreground = App.GetResource<IBrush>("ThemeForegroundLowBrush");
            HistorySelected.Opacity = 0;
            HistoryRoot.IsVisible = false;

            StatisticsText.Foreground = App.GetResource<IBrush>("ThemeForegroundBrush");
            StatisticsSelected.Opacity = 1;
            StatisticsRoot.IsVisible = true;
        }

        void HistoryClick(object sender, PointerReleasedEventArgs e) {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased) return;

            e.Handled = true;

            StatisticsText.Foreground = App.GetResource<IBrush>("ThemeForegroundLowBrush");
            StatisticsSelected.Opacity = 0;
            StatisticsRoot.IsVisible = false;

            HistoryText.Foreground = App.GetResource<IBrush>("ThemeForegroundBrush");
            HistorySelected.Opacity = 1;
            HistoryRoot.IsVisible = true;
        }
    }
}
