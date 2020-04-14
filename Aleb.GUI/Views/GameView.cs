using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Prompts;
using Avalonia.Styling;

namespace Aleb.GUI.Views {
    public class GameView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            UserText = this.Get<DockPanel>("Root").Children.OfType<UserInGame>().ToList();

            Cards = this.Get<StackPanel>("Cards");
            
            TableSegments = Enumerable.Range(0, 4).Select(i => this.Get<Border>($"Table{i}")).ToList();
            CardTableSegments = Enumerable.Range(0, 4).Select(i => this.Get<Border>($"CardTable{i}")).ToList();

            alert = this.Get<StackPanel>("Alert");
            prompt = this.Get<Border>("Prompt");
            trump = this.Get<Border>("Trump");
        }

        List<UserInGame> UserText;
        StackPanel Cards;

        List<Border> TableSegments, CardTableSegments;
        
        StackPanel alert;
        Border prompt, trump;

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

        void Table(int index, Control value) {
            if (value != null) {
                int position = Utilities.Modulo(index - You, 4);
                
                value.HorizontalAlignment = TableH[position];
                value.VerticalAlignment = TableV[position];

                if (value is CardStack cardStack) cardStack.ApplyPosition(position);
            }

            TableSegments[index].Child = value;
        }

        void CardTable(int index, CardImage value) {
            if (value != null) {
                int position = Utilities.Modulo(index - You, 4);
                
                value.HorizontalAlignment = TableH.Rotate(2).ElementAt(position);
                value.VerticalAlignment = TableV.Rotate(2).ElementAt(position);

                value.MaxHeight = 130;
                value.Margin = new Thickness(5);

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
        }

        public GameView() => throw new InvalidOperationException();

        public GameView(List<string> names) {
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
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted += GameStarted;

            Network.TrumpNext += TrumpNext;
            Network.TrumpChosen += TrumpChosen;

            Network.YouDeclared += YouDeclared;
            Network.PlayerDeclared += PlayerDeclared;
            Network.WinningDeclaration += WinningDeclaration;

            Network.YouPlayed += YouPlayed;
            Network.CardPlayed += CardPlayed;
            Network.TableComplete += TableComplete;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted -= GameStarted;
            
            Network.TrumpNext -= TrumpNext;
            Network.TrumpChosen -= TrumpChosen;

            Network.YouDeclared -= YouDeclared;
            Network.PlayerDeclared -= PlayerDeclared;
            Network.WinningDeclaration -= WinningDeclaration;

            Network.YouPlayed -= YouPlayed;
            Network.CardPlayed -= CardPlayed;
            Network.TableComplete -= TableComplete;
        }

        GameState State;
        int You;

        int _dealer;
        int Dealer {
            get => _dealer;
            set {
                _dealer = value;
                
                for (int i = 0; i < 4; i++)
                    UserText[i].DealerIcon.IsVisible = i == Dealer;
            }
        }

        int lastPlaying, lastInTable;

        void SetPlaying(int playing) {
            lastPlaying = playing;

            for (int i = 0; i < 4; i++)
                UserText[i].Playing = i == playing;

            if (State == GameState.Bidding)
                Prompt = (playing == You)? new BidPrompt(playing == Dealer) : null;

            else if (State == GameState.Declaring)
                Prompt = (playing == You)? new DeclarePrompt(DeclareSelected) : null;
        }

        void NextPlaying() => SetPlaying(Utilities.Modulo(lastPlaying + 1, 4));

        bool[] DeclareSelected;

        void CardClicked(CardImage sender) {
            int index = Cards.Children.IndexOf(sender);

            if (State == GameState.Declaring && DeclareSelected != null) {
                DeclareSelected[index] = !DeclareSelected[index];

                int margin = 15 * Convert.ToInt32(DeclareSelected[index]);
                sender.Margin = new Thickness(0, -margin, 0, margin);
            
            } else if (State == GameState.Playing) {
                if (lastPlaying == You) Requests.PlayCard(index, false); //todo bela
                else Table(You, new TextOverlay("Niste na potezu", 3000));
            }
        }

        public void GameStarted(int dealer, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => GameStarted(dealer, cards));
                return;
            }

            Dealer = dealer;

            State = GameState.Bidding;

            CreateCards(cards);

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
        }

        public void TrumpNext() {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TrumpNext());
                return;
            }

            if (State != GameState.Bidding) return;

            Table(lastPlaying, new TextOverlay("Dalje"));

            NextPlaying();
        }

        public void TrumpChosen(Suit trump, List<int> cards) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TrumpChosen(trump, cards));
                return;
            }

            if (State != GameState.Bidding) return;

            ClearTable();

            Trump = new Trump(trump, UserText[lastPlaying].Text);

            DeclareSelected = new bool[8];
            State = GameState.Declaring;

            CreateCards(cards);

            SetPlaying(Utilities.Modulo(Dealer + 1, 4));
        }

        public void YouDeclared(bool result) {
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

        public void PlayerDeclared(int value) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => PlayerDeclared(value));
                return;
            }

            if (State != GameState.Declaring) return;
            
            Table(lastPlaying, new TextOverlay((value != 0)? value.ToString() : "Dalje"));

            if (lastPlaying == Dealer) SetPlaying(-1);
            else NextPlaying();
        }

        public void WinningDeclaration(int player, int value, List<int> calls, List<int> teammateCalls) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => WinningDeclaration(player, value, calls, teammateCalls));
                return;
            }

            if (State != GameState.Declaring) return;

            int noDeclarations = value != 0? 0 : 1500;

            Task.Delay(1500 - noDeclarations).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => {
                ClearTable();

                foreach (CardImage card in Cards.Children.OfType<CardImage>())
                    card.Margin = new Thickness(0);

                if (value != 0) {
                    Table(player, new CardStack(calls));
                    Table(Utilities.Modulo(player + 2, 4), new CardStack(teammateCalls));

                    Alert = new TextOverlay(value.ToString());

                } else Alert = new TextOverlay("Nema zvanja");

                Task.Delay(3000 - noDeclarations).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => {
                    ClearTable();
                    Alert = null;
                    State = GameState.Playing;

                    SetPlaying(Utilities.Modulo(Dealer + 1, 4));
                    lastInTable = Utilities.Modulo(lastPlaying - 1, 4);
                }));
            }));
        }

        public void YouPlayed(int index) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => YouPlayed(index));
                return;
            }

            if (State != GameState.Playing) return;

            if (index != -1) Cards.Children.RemoveAt(index);
            else Table(You, new TextOverlay("Neispravna karta", 3000));
        }

        public void CardPlayed(int card) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => CardPlayed(card));
                return;
            }

            if (State != GameState.Playing) return;

            CardTable(lastPlaying, new CardImage(card));

            if (lastPlaying == lastInTable) SetPlaying(-1);
            else NextPlaying();
        }

        public void TableComplete(int winner, List<int> calls, List<int> played) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => TableComplete(winner, calls, played));
                return;
            }

            if (State != GameState.Playing) return;

            Task.Delay(2000).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => {
                ClearTable();

                SetPlaying(winner);
                lastInTable = Utilities.Modulo(lastPlaying - 1, 4);
            }));
        }
    }
}
