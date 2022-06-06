using System;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Aleb.Common;

namespace Aleb.GUI.Components {
    public class MatchMetadata: IconButton {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Alternating = this.Get<Grid>("Alternating");
            
            VictoryColor = this.Get<Path>("VictoryColor");
            Victory = this.Get<TextBlock>("Victory");

            Score = new TextBlock[] {
                this.Get<TextBlock>("ScoreLeft"),
                this.Get<TextBlock>("ScoreRight")
            };

            Results = this.Get<StackPanel>("ResultsLeft").Children.OfType<UserInList>()
                .Concat(this.Get<StackPanel>("ResultsRight").Children.OfType<UserInList>()).ToArray();
        }

        Grid Alternating;
        
        public bool AlternatingColor {
            set => Alternating.IsVisible = value;
        }

        Path VictoryColor;
        TextBlock Victory;
        
        TextBlock[] Score;
        UserInList[] Results;

        protected override IBrush Fill {
            get => Background;
            set => Background = value;
        }

        public MatchMetadata() => throw new InvalidOperationException();

        public MatchMetadata(RecordedMatch match, string viewing)
        //: base("ThemeBackgroundBrush", "ThemeControlLowBrush", "ThemeControlHighBrush", "ThemeControlMidHighBrush") {
        : base("ThemeBackgroundBrush", "ThemeBackgroundBrush", "ThemeControlHighBrush", "ThemeControlMidHighBrush") {
            InitializeComponent();
            Enabled = false;

            int rotate = Math.Max(0, match.Users.ToList().IndexOf(viewing)) >> 1 & 1;
            bool win = match.Score[rotate] > match.Score[1 - rotate];
            
            VictoryColor.Fill = SolidColorBrush.Parse(win? "#00DD00" : "#DD0000");
            Victory.Text = win? "W" : "L";

            foreach (var (text, pts) in Score.Zip(match.Score.Rotate(rotate)))
                text.Text = pts >= Consts.BelotValue? "Belot" : pts.ToString();

            foreach (var (text, user) in Results.Zip(match.Users.Rotate(rotate * 2))) {
                text.Text = user;
                if (user == viewing)
                    text.FontWeight = FontWeight.Bold;
            }

            ToolTip.SetTip(this, $"{match.RoomName}, {match.Date.ToLocalTime():dd.MM.yyyy. HH:mm}");
        }
    }
}
