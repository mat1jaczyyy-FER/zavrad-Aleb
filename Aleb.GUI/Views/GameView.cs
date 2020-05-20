using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Prompts;
using System.Diagnostics;

namespace Aleb.GUI.Views {
    public class GameView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            UserText = this.Get<DockPanel>("Root").Children.OfType<UserInGame>().ToList();

            Cards = this.Get<StackPanel>("Cards");
            Rounds = this.Get<StackPanel>("Rounds");
            Score = this.Get<StackPanel>("Score");

            TitleRow = this.Get<RoundRow>("TitleRow");
            Declarations = this.Get<RoundRow>("Declarations");
            CurrentRound = this.Get<RoundRow>("CurrentRound");
            TotalRound = this.Get<RoundRow>("TotalRound");
            Total = this.Get<RoundRow>("Total");
            
            TableSegments = Enumerable.Range(0, 4).Select(i => this.Get<Border>($"Table{i}")).ToList();
            CardTableSegments = Enumerable.Range(0, 4).Select(i => this.Get<Border>($"CardTable{i}")).ToList();

            alert = this.Get<StackPanel>("Alert");
            prompt = this.Get<Border>("Prompt");
            trump = this.Get<Border>("Trump");

            TimeElapsed = this.Get<TextBlock>("TimeElapsed");
        }

        List<UserInGame> UserText;
        StackPanel Cards, Rounds, Score;

        RoundRow TitleRow, Declarations, CurrentRound, TotalRound, Total;

        List<Border> TableSegments, CardTableSegments;
        
        StackPanel alert;
        Border prompt, trump;

        Stopwatch timer = new Stopwatch();
        DispatcherTimer Timer;
        TextBlock TimeElapsed;

        void UpdateTime(object sender, EventArgs e)
            => TimeElapsed.Text = $"{((int)timer.Elapsed.TotalHours > 0? $"{(int)timer.Elapsed.TotalHours}:" : "")}{timer.Elapsed.Minutes:00}:{timer.Elapsed.Seconds:00}";

        Control Prompt {
            get => (Control)prompt.Child;
            set {
                if (value is TextOverlay textOverlay) {
                    textOverlay.HorizontalAlignment = HorizontalAlignment.Center;
                    textOverlay.VerticalAlignment = VerticalAlignment.Center;
                }

                prompt.Child = value;
                prompt.Background = (value is TextOverlay)? null : (IBrush)Application.Current.FindResource("ThemeControlMidHighBrush");
                prompt.IsVisible = Prompt != null;
            }
        }

        Control Alert {
            get => (Control)alert.Children.FirstOrDefault();
            set {
                alert.Children.Clear();
                alert.Children.Add(value);
            }
        }

        Trump Trump {
            get => (Trump)trump.Child;
            set => trump.Child = value;
        }

        HorizontalAlignment[] TableH = new HorizontalAlignment[] {
            HorizontalAlignment.Center,
            HorizontalAlignment.Right,
            HorizontalAlignment.Center,
            HorizontalAlignment.Left
        };

        VerticalAlignment[] TableV = new VerticalAlignment[] {
            VerticalAlignment.Bottom,
            VerticalAlignment.Center,
            VerticalAlignment.Top,
            VerticalAlignment.Center
        };

        int Position(int index) => Utilities.Modulo(index - You, 4);

        void Table(int index, Control value) {
            if (value != null) {
                int position = Position(index);
                
                value.HorizontalAlignment = TableH[position];
                value.VerticalAlignment = TableV[position];

                if (value is CardStack cardStack) cardStack.ApplyPosition(position);
            }

            TableSegments[index].Child = value;
        }

        void CardTable(int index, CardImage value) {
            if (value != null) {
                int position = Position(index);
                
                value.HorizontalAlignment = TableH.Rotate(2).ElementAt(position);
                value.VerticalAlignment = TableV.Rotate(2).ElementAt(position);
                
                value.Cursor = Cursor.Default;
                value.MaxHeight = 128;

                CardTableSegments[index].Child = value;

            } else CardTableSegments[index].Child.Opacity = 0;
        }

        void ClearTable() {
            for (int i = 0; i < 4; i++) {
                Table(i, null);
                CardTable(i, null);
            }
        }

        CardImage CreateCard(int card) {
            CardImage cardImage = new CardImage(card);
            cardImage.Clicked += CardClicked;
            return cardImage;
        }

        void CreateCards(List<int> cards) {
            Cards.Children.Clear();

            foreach (int card in cards)
                Cards.Children.Add(CreateCard(card));

            while (Cards.Children.Count < 8)
                Cards.Children.Add(CreateCard(32));

            Cards.Parent.Opacity = 1;  
        }

        static List<string> FailScore(List<int> score, bool fail)
            => score.Select(i => (fail && i == 0)? "—" : i.ToString()).ToList();

        static List<string> emptyRow = new List<string>() { "", "" };

        void UpdateRow<T>(RoundRow row, List<T> values, bool autoTeams = true) {
            int team = autoTeams? Team : 0;
            row.Left = values[team].ToString();
            row.Right = values[1 - team].ToString();
        }

        void UpdateTitleRow(bool mivi) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => UpdateTitleRow(mivi));
                return;
            }

            UpdateRow(TitleRow, (mivi
                ? new List<string>() { "Mi", "Vi" }
                : new List<string>() { "Vi", "Oni" }
            ), false);
        }

        void UpdateCurrentRound(List<int> calls, List<int> played) {
            UpdateRow(Declarations, calls);
            UpdateRow(CurrentRound, played);
            UpdateRow(TotalRound, calls.Zip(played).Select(t => t.First + t.Second).ToList());
        }

        void UpdateCurrentRound(List<int> final, bool fail) {
            UpdateRow(Declarations, emptyRow);
            UpdateRow(CurrentRound, emptyRow);
            UpdateRow(TotalRound, FailScore(final, fail));
        }

        void UpdateCurrentRound(int called, int team) {
            List<int> calls = new List<int>() {
                (team == Team)? called : 0,
                (team != Team)? called : 0
            };

            UpdateRow(Declarations, calls);
            UpdateRow(CurrentRound, new List<int>() { 0, 0 });
            UpdateRow(TotalRound, calls);
        }

        void ClearCurrentRound() {
            UpdateRow(Declarations, emptyRow);
            UpdateRow(CurrentRound, emptyRow);
            UpdateRow(TotalRound, emptyRow);
        }

        void ScoreEnter(object sender, PointerEventArgs e) => Rounds.IsVisible = true;
        void ScoreLeave(object sender, PointerEventArgs e) => Rounds.IsVisible = false;

        public GameView() => throw new InvalidOperationException();

        public GameView(List<string> names, int dealer, List<int> cards) {
            InitializeComponent();

            UserText.Swap(1, 2);
            names.Swap(1, 2);

            foreach (var (text, name) in UserText.Zip(names.RotateWith(i => i == App.User.Name)))
                text.Text = name;

            int position = UserText.Select(i => i.Text).ToList().IndexOf(names[0]);
            
            UserText = UserText.Rotate(position).ToList();
            TableSegments = TableSegments.Rotate(position).ToList();
            CardTableSegments = CardTableSegments.Rotate(position).ToList();

            for (int i = 0; i < 4; i++)
                CardTable(i, new CardImage(32) { Opacity = 0 });

            You = UserText.IndexOf(UserText.First(i => i.Text == App.User.Name));

            UpdateTitleRow(Preferences.MiVi);
            Preferences.MiViChanged += UpdateTitleRow;
            
            timer.Start();

            UpdateTime(null, EventArgs.Empty);
            Timer = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 0, 0, 500)
            };
            Timer.Tick += UpdateTime;
            Timer.Start();

            GameStarted(dealer, cards);
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted += GameStarted;

            Network.TrumpNext += TrumpNext;
            Network.TrumpChosen += TrumpChosen;

            Network.YouDeclared += YouDeclared;
            Network.PlayerDeclared += PlayerDeclared;

            Network.WinningDeclaration += WinningDeclaration;
            Network.StartPlayingCards += StartPlayingCards;

            Network.YouPlayed += YouPlayed;
            Network.AskBela += AskBela;
            Network.CardPlayed += CardPlayed;

            Network.TableComplete += TableComplete;
            Network.ContinuePlayingCards += ContinuePlayingCards;

            Network.FinalScores += FinalScores;
            Network.TotalScore += TotalScore;

            Network.GameFinished += GameFinished;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Preferences.MiViChanged -= UpdateTitleRow;

            Network.GameStarted -= GameStarted;
            
            Network.TrumpNext -= TrumpNext;
            Network.TrumpChosen -= TrumpChosen;

            Network.YouDeclared -= YouDeclared;
            Network.PlayerDeclared -= PlayerDeclared;

            Network.WinningDeclaration -= WinningDeclaration;
            Network.StartPlayingCards -= StartPlayingCards;

            Network.YouPlayed -= YouPlayed;
            Network.AskBela -= AskBela;
            Network.CardPlayed -= CardPlayed;

            Network.TableComplete -= TableComplete;
            Network.ContinuePlayingCards -= ContinuePlayingCards;

            Network.FinalScores -= FinalScores;
            Network.TotalScore -= TotalScore;

            Network.GameFinished -= GameFinished;

            timer.Stop();
            Timer.Stop();
            Timer.Tick -= UpdateTime;
        }

        GameState State;
        int You;
        int Team => You % 2;

        int _dealer;
        int Dealer {
            get => _dealer;
            set {
                _dealer = value;
                
                for (int i = 0; i < 4; i++)
                    UserText[i].DealerIcon.IsVisible = i == Dealer;
            }
        }

        bool[] DeclareSelected;

        int lastPlaying, lastInTable, selectedTrump, roundNum;
        CardImage lastPlayed;

        void SetPlaying(int playing) {
            lastPlaying = playing;

            for (int i = 0; i < 4; i++)
                UserText[i].Playing = i == playing;

            if (playing == You) {
                Audio.YourTurn();
                
                if (State == GameState.Bidding)
                    Prompt = new BidPrompt(playing == Dealer);

                else if (State == GameState.Declaring)
                    Prompt = new DeclarePrompt(DeclareSelected);

            } else Prompt = null;
        }

        void NextPlaying() => SetPlaying(Utilities.Modulo(lastPlaying + 1, 4));

        void CardClicked(CardImage sender) {
            int index = Cards.Children.IndexOf(sender);

            if (State == GameState.Bidding && lastPlaying == You && index >= 6) {
                Requests.TalonBid(index - 6);

            } if (State == GameState.Declaring && DeclareSelected != null) {
                DeclareSelected[index] = !DeclareSelected[index];

                int margin = 15 * Convert.ToInt32(DeclareSelected[index]);
                sender.Margin = new Thickness(0, -margin, 0, margin);
            
            } else if (State == GameState.Playing && Prompt == null) {
                if (lastPlaying == You) {
                    lastPlayed = sender;
                    Requests.PlayCard(index);

                } else Table(You, new TextOverlay("Niste na potezu", 3000));
            }
        }

        void GameStarted(int dealer, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameStarted(dealer, cards));
                return;
            }

            ClearCurrentRound();
            ClearTable();

            Dealer = dealer;

            State = GameState.Bidding;

            CreateCards(cards);
            Trump = null;

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
        }

        void TrumpNext() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TrumpNext());
                return;
            }

            if (State != GameState.Bidding) return;

            Table(lastPlaying, new TextOverlay("Dalje"));

            NextPlaying();
        }

        void TrumpChosen(Suit trump, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TrumpChosen(trump, cards));
                return;
            }

            if (State != GameState.Bidding) return;

            ClearTable();

            Trump = new Trump(trump, UserText[lastPlaying].Text);
            selectedTrump = lastPlaying;

            DeclareSelected = new bool[8];
            State = GameState.Declaring;

            CreateCards(cards);

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
        }

        void YouDeclared(bool result) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => YouDeclared(result));
                return;
            }

            if (State != GameState.Declaring) return;
            
            if (result) {
                Table(You, null);
                DeclareSelected = null;
                Prompt = null;

            } else Table(You, new TextOverlay("Nevažeće zvanje", 3000));
        }

        void PlayerDeclared(int value) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => PlayerDeclared(value));
                return;
            }

            if (State != GameState.Declaring) return;
            
            Table(lastPlaying, new TextOverlay((value != 0)? value.ToString() : "Dalje"));

            if (lastPlaying == Dealer) SetPlaying(-1);
            else NextPlaying();
        }

        void WinningDeclaration(int player, int value, List<int> calls, List<int> teammateCalls) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => WinningDeclaration(player, value, calls, teammateCalls));
                return;
            }

            if (State != GameState.Declaring) return;

            ClearTable();

            foreach (CardImage card in Cards.Children.OfType<CardImage>())
                card.Margin = new Thickness(0);

            Score.Opacity = 0;

            if (value != 0) {
                Table(player, new CardStack(calls));
                Table(Utilities.Modulo(player + 2, 4), new CardStack(teammateCalls));

                Alert = new TextOverlay(value.ToString());

            } else Alert = new TextOverlay("Nema zvanja");

            UpdateCurrentRound(value, Position(player) % 2);
        }

        void StartPlayingCards() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(StartPlayingCards);
                return;
            }

            if (State != GameState.Declaring) return;

            ClearTable();
            Alert = null;
            State = GameState.Playing;

            Score.Opacity = 1;

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
            lastInTable = Utilities.Modulo(lastPlaying - 1, 4);
        }

        void YouPlayed(int index) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => YouPlayed(index));
                return;
            }

            if (State != GameState.Playing) return;

            if (index != -1) {
                Cards.Children.RemoveAt(index);
                
                if (!Cards.Children.Any()) Cards.Parent.Opacity = 0;

                Prompt = null;

            } else Table(You, new TextOverlay("Neispravna karta", 3000));
        }

        void AskBela() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(AskBela);
                return;
            }

            if (State != GameState.Playing) return;

            lastPlayed.Margin = new Thickness(0, -15, 0, 15);

            Prompt = new BelaPrompt();
        }

        void CardPlayed(int card, bool bela) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => CardPlayed(card, bela));
                return;
            }

            if (State != GameState.Playing) return;
            
            Table(lastPlaying, null);
            CardTable(lastPlaying, new CardImage(card));

            if (bela)
                Table(lastPlaying, new TextOverlay("Bela"));

            if (lastPlaying == lastInTable) SetPlaying(-1);
            else NextPlaying();
        }

        void TableComplete(List<int> calls, List<int> played, bool fail) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TableComplete(calls, played, fail));
                return;
            }

            if (State != GameState.Playing) return;

            UpdateCurrentRound(calls, played);

            if (fail) Table(selectedTrump, new TextOverlay($"Pali {(Position(selectedTrump) % 2 == 0? "ste" : "su")}"));
        }

        void ContinuePlayingCards(int winner) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => ContinuePlayingCards(winner));
                return;
            }

            if (State != GameState.Playing) return;
            
            ClearTable();

            SetPlaying(winner);
            lastInTable = Utilities.Modulo(lastPlaying - 1, 4);
        }

        void FinalScores(List<int> final, bool fail) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => FinalScores(final, fail));
                return;
            }

            if (State != GameState.Playing) return;
            
            ClearTable();

            SetPlaying(-1);

            UpdateCurrentRound(final, fail);
        }

        void TotalScore(List<int> final, bool fail, List<int> total) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TotalScore(final, fail, total));
                return;
            }

            if (State != GameState.Playing) return;
            
            RoundRow row = new RoundRow() { Legend = $"{++roundNum}." };
            
            bool temp = Rounds.IsVisible;
            Rounds.IsVisible = false;
            Rounds.IsVisible = temp;

            UpdateRow(row, FailScore(final, fail));
            Rounds.Children.Add(row);

            UpdateRow(Total, total);
            Total.IsVisible = true;
        }

        void GameFinished(List<int> score, Room room) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameFinished(score, room));
                return;
            }

            if (State != GameState.Playing) return;
            
            App.MainWindow.View = new ResultsView(score, room);
        }
    }
}
