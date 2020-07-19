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

            FinalCard = this.Get<DockPanel>("FinalCard").Children.OfType<TextOverlay>().ToList();

            TimeElapsed = this.Get<TextBlock>("TimeElapsed");
        }

        List<UserInGame> UserText;
        StackPanel Cards, Rounds, Score;

        RoundRow TitleRow, Declarations, CurrentRound, TotalRound, Total;

        List<Border> TableSegments, CardTableSegments;
        
        StackPanel alert;
        Border prompt, trump;

        List<TextOverlay> FinalCard;

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

        static List<string> FailScore(FinalizedRound score)
            => score.Score.Select(i => (score.Fail && i == 0)? "—" : i.ToString()).ToList();

        static List<string> emptyRow = new List<string>() { "", "" };

        List<int> discScores = new List<int>() { 0, 0 };

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

        void UpdateCurrentRound(FinalizedRound final) {
            UpdateRow(Declarations, emptyRow);
            UpdateRow(CurrentRound, emptyRow);
            UpdateRow(TotalRound, FailScore(final));
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
        
        void FinalCardsEnter(object sender, PointerEventArgs e) => FinalCardsUpdate(sender, true);
        void FinalCardsLeave(object sender, PointerEventArgs e) => FinalCardsUpdate(sender, false);
        
        void FinalCardsUpdate(object sender, bool visible) {
            if (!(sender is Control source)) return;
            if (!(source.Parent is DockPanel parent)) return;

            FinalCard[parent.Children.IndexOf(source)].IsVisible = visible;
        }

        bool ShouldReconnect;

        void InitNames(List<string> names) {
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

            ShouldReconnect = false;
        }

        public GameView() {
            InitializeComponent();

            UpdateTitleRow(Preferences.MiVi);
            Preferences.MiViChanged += UpdateTitleRow;
            
            timer.Start();

            UpdateTime(null, EventArgs.Empty);
            Timer = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            Timer.Tick += UpdateTime;
            Timer.Start();

            ShouldReconnect = true;

            Discord.Info.Details = $"U igri - {Discord.Info.State}";
            Discord.Info.Party = null;
            Discord.Info.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
        }

        public GameView(List<string> names): this() => InitNames(names);

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted += GameStarted;
            Network.Reconnect += Reconnect;

            Network.TrumpNext += TrumpNext;
            Network.TrumpChosen += TrumpChosen;
            Network.FullCards += FullCards;

            Network.YouDeclared += YouDeclared;
            Network.PlayerDeclared += PlayerDeclared;

            Network.WinningDeclaration += WinningDeclaration;
            Network.StartPlayingCards += StartPlayingCards;

            Network.AskBela += AskBela;
            Network.YouPlayed += YouPlayed;
            Network.CardPlayed += CardPlayed;

            Network.TableComplete += TableComplete;
            Network.ContinuePlayingCards += ContinuePlayingCards;

            Network.FinalScores += FinalScores;
            Network.FinalCards += FinalCards;
            Network.TotalScore += TotalScore;

            Network.GameFinished += GameFinished;

            if (ShouldReconnect) Requests.Reconnecting();
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Preferences.MiViChanged -= UpdateTitleRow;

            Network.GameStarted -= GameStarted;
            Network.Reconnect -= Reconnect;
            
            Network.TrumpNext -= TrumpNext;
            Network.TrumpChosen -= TrumpChosen;
            Network.FullCards -= FullCards;

            Network.YouDeclared -= YouDeclared;
            Network.PlayerDeclared -= PlayerDeclared;

            Network.WinningDeclaration -= WinningDeclaration;
            Network.StartPlayingCards -= StartPlayingCards;

            Network.AskBela -= AskBela;
            Network.YouPlayed -= YouPlayed;
            Network.CardPlayed -= CardPlayed;

            Network.TableComplete -= TableComplete;
            Network.ContinuePlayingCards -= ContinuePlayingCards;

            Network.FinalScores -= FinalScores;
            Network.FinalCards -= FinalCards;
            Network.TotalScore -= TotalScore;

            Network.GameFinished -= GameFinished;

            timer.Stop();
            Timer.Stop();
            Timer.Tick -= UpdateTime;
        }

        GameState _state;
        GameState State {
            get => _state;
            set {
                _state = value;

                if (_state == GameState.Bidding) Discord.Info.State = "Bira aduta";
                if (_state == GameState.Declaring) Discord.Info.State = "Zvanja";
                if (_state == GameState.Playing) Discord.Info.State = "Karta";

                if (Dealer == You) Discord.Info.State += " na musu";
                Discord.Info.State += $" ({discScores[0]} - {discScores[1]})";
            }
        }

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

        int lastPlaying, lastInTable, selectedTrump;
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

        bool playWaiting;

        void CardClicked(CardImage sender) {
            int index = Cards.Children.IndexOf(sender);

            if (State == GameState.Bidding && lastPlaying == You && Dealer == You && index >= 6) {
                Requests.TalonBid(index - 6);

            } if (State == GameState.Declaring && DeclareSelected != null) {
                DeclareSelected[index] = !DeclareSelected[index];

                int margin = 15 * Convert.ToInt32(DeclareSelected[index]);
                sender.Margin = new Thickness(0, -margin, 0, margin);
            
            } else if (State == GameState.Playing && Prompt == null) {
                if (lastPlaying == You) {
                    if (playWaiting) return;
                    playWaiting = true;

                    lastPlayed = sender;
                    Requests.PlayCard(index);

                } else Table(You, new TextOverlay("Niste na potezu", 3000));
            }
        }

        public void GameStarted(int dealer, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameStarted(dealer, cards));
                return;
            }

            ClearCurrentRound();
            ClearTable();

            Dealer = dealer;

            State = GameState.Bidding;

            Discord.Logo.SmallImageKey = null;
            Discord.Logo.SmallImageText = null;

            CreateCards(cards);
            Trump = null;

            Score.Opacity = 1;

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

        void TrumpChosen(Suit trump) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TrumpChosen(trump));
                return;
            }

            if (State != GameState.Bidding) return;

            ClearTable();

            Trump = new Trump(trump, UserText[lastPlaying].Text);
            selectedTrump = lastPlaying;

            Discord.Logo.SmallImageKey = trump.ToString().ToLower();
            Discord.Logo.SmallImageText = lastPlaying % 2 == Team? "Zvao": "Ruši";

            DeclareSelected = new bool[8];
            State = GameState.Declaring;

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
        }

        void FullCards(List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => FullCards(cards));
                return;
            }

            if (State != GameState.Bidding) return;

            CreateCards(cards);
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

        void AskBela() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(AskBela);
                return;
            }

            if (State != GameState.Playing) return;

            lastPlayed.Margin = new Thickness(0, -15, 0, 15);

            Prompt = new BelaPrompt();
        }

        void YouPlayed(int card) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => YouPlayed(card));
                return;
            }

            if (State != GameState.Playing) return;

            playWaiting = false;

            if (card != -1) {
                Cards.Children.RemoveAt(card);
                
                if (!Cards.Children.Any()) Cards.Parent.Opacity = 0;

                Prompt = null;

            } else Table(You, new TextOverlay("Neispravna karta", 3000));
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

        void TableComplete(List<int> calls, FinalizedRound played) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TableComplete(calls, played));
                return;
            }

            if (State != GameState.Playing) return;

            UpdateCurrentRound(calls, played.Score);

            if (played.Fail) Table(selectedTrump, new TextOverlay(new Image() {
                Height = 70,
                Source = App.GetImage("Edgar")
            }));
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

        void FinalScores(FinalizedRound final) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => FinalScores(final));
                return;
            }

            if (State != GameState.Playing) return;
            
            ClearTable();

            SetPlaying(-1);

            UpdateCurrentRound(final);
        }

        void FinalCards(int player, List<int> cards, List<int> talon) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => FinalCards(player, cards, talon));
                return;
            }

            if (State != GameState.Playing) return;

            Score.Opacity = 0;

            FinalCard[player].SetControl(new CardStack(cards, talon));
        }

        void TotalScore(FinalizedRound final, List<int> total) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TotalScore(final, total));
                return;
            }

            if (State != GameState.Playing) return;
            
            RoundRow row = new RoundRow() { Icon = $"Suits/{final.Suit}" };
            
            bool temp = Rounds.IsVisible;
            Rounds.IsVisible = false;
            Rounds.IsVisible = temp;

            UpdateRow(row, FailScore(final));
            Rounds.Children.Add(row);

            UpdateRow(Total, discScores = total);
            Total.IsVisible = true;
        }

        void GameFinished(List<int> score, Room room, string password) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameFinished(score, room, password));
                return;
            }

            if (State != GameState.Playing) return;

            room.Password = password;
            
            App.MainWindow.View = new ResultsView(score, room);
        }

        void Reconnect(Room room, List<FinalizedRound> history) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => Reconnect(room, history));
                return;
            }

            App.MainWindow.Title = room.Name;
            Discord.Info.Details = $"U igri - {room.Name}";

            InitNames(room.Users.Select(i => i.Name).ToList());

            List<int> total = Enumerable.Range(0, 2).Select(i => history.Sum(j => j.Score[i])).ToList();

            State = GameState.Playing;

            foreach (FinalizedRound round in history)
                TotalScore(round, total);

            State = GameState.Bidding;
        }
    }
}
