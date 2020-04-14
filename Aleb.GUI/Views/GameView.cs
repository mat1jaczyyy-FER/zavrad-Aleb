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

namespace Aleb.GUI.Views {
    public class GameView: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            UserText = this.Get<DockPanel>("Root").Children.OfType<UserInGame>().ToList();

            Cards = this.Get<StackPanel>("Cards");
            
            TableSegments = Enumerable.Range(0, 4).Select(i => this.Get<Border>($"Table{i}")).ToList();

            prompt = this.Get<Border>("Prompt");
        }

        List<UserInGame> UserText;
        StackPanel Cards;

        List<Border> TableSegments;

        Border prompt;

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

        void Table(int index, Control value) {
            if (value != null) {
                int position = Utilities.Modulo(index - You, 4);
            
                if (position % 2 == 0) {
                    value.HorizontalAlignment = HorizontalAlignment.Center;
                    value.VerticalAlignment = (position == 0)? VerticalAlignment.Bottom : VerticalAlignment.Top;

                } else {
                    value.HorizontalAlignment = (position == 1)? HorizontalAlignment.Right : HorizontalAlignment.Left;
                    value.VerticalAlignment = VerticalAlignment.Center;
                }

                if (value is CardStack cardStack) cardStack.ApplyPosition(position);
            }

            TableSegments[index].Child = value;
        }

        void ClearTable() {
            for (int i = 0; i < 4; i++)
                Table(i, null);
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

            TableSegments = TableSegments.Rotate(UserText.Select(i => i.Text).ToList().IndexOf(names[0])).ToList();
            UserText = UserText.RotateWith(i => i.Text == names[0]).ToList();

            You = UserText.IndexOf(UserText.First(i => i.Text == App.User.Name));
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted += GameStarted;

            Network.TrumpNext += TrumpNext;
            Network.TrumpChosen += TrumpChosen;

            Network.YouDeclared += YouDeclared;
            Network.PlayerDeclared += PlayerDeclared;
            Network.WinningDeclaration += WinningDeclaration;
        }

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Network.GameStarted -= GameStarted;
            
            Network.TrumpNext -= TrumpNext;
            Network.TrumpChosen -= TrumpChosen;

            Network.YouDeclared -= YouDeclared;
            Network.PlayerDeclared -= PlayerDeclared;
            Network.WinningDeclaration -= WinningDeclaration;
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

        int lastPlaying;

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

            if (lastPlaying != Dealer)
                NextPlaying();
        }

        public void WinningDeclaration(int player, int value, List<int> calls, List<int> teammateCalls) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.InvokeAsync(() => WinningDeclaration(player, value, calls, teammateCalls));
                return;
            }

            if (State != GameState.Declaring) return;

            SetPlaying(-1);

            int noDeclarations = value != 0? 0 : 1500;

            Task.Delay(1500 - noDeclarations).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => {
                ClearTable();

                foreach (CardImage card in Cards.Children.OfType<CardImage>())
                    card.Margin = new Thickness(0);

                if (value != 0) {
                    Table(player, new CardStack(calls));
                    Table(Utilities.Modulo(player + 2, 4), new CardStack(teammateCalls));

                    Prompt = new TextOverlay(value.ToString());

                } else Prompt = new TextOverlay("Nema zvanja");

                Task.Delay(3000 - noDeclarations).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => {
                    ClearTable();
                    Prompt = null;
                    State = GameState.Playing;

                    SetPlaying(Utilities.Modulo(Dealer + 1, 4));
                }));
            }));
        }
    }
}
