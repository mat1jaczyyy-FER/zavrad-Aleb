using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Game {
        TimeSpan StartTime = AlebClient.TimeNow;

        public void Flush() {
            foreach (Player player in Players)
                player.Flush();

            Spectators.Flush();
        }

        void Broadcast(int delay, string command, params dynamic[] args) {
            foreach (Player player in Players)
                player.SendMessage(delay, command, args);

            Spectators.SendMessage(delay, command, args);
        }

        void Broadcast(string command, params dynamic[] args)
            => Broadcast(0, command, args);

        Player[] Players = new Player[4];
        Recorder Spectators;
        public RecordedMatch Recording => Spectators.Recording;

        GameState State;

        Table Table;

        Player Dealer, Current;

        public List<Round> History { get; private set; } = new List<Round>();
        public int[] Score => Enumerable.Range(0, 2).Select(i => History.Where(x => x.Finalized).Sum(x => x.Played[i])).ToArray();

        Room Room;

        public Game(Room room) {
            Room = room;

            Players[0] = Room.Users[0].ToPlayer(this);
            Players[1] = Room.Users[2].ToPlayer(this);
            Players[2] = Room.Users[1].ToPlayer(this);
            Players[3] = Room.Users[3].ToPlayer(this);

            for (int i = 0; i < 4; i++) {
                Players[i].Previous = Players[Utilities.Modulo(i - 1, 4)];
                Players[i].Next = Players[Utilities.Modulo(i + 1, 4)];
                Players[i].Teammate = Players[Utilities.Modulo(i + 2, 4)];
                Players[i].Team = i % 2;
            }

            Dealer = Players[2];

            Spectators = new Recorder(Room.Spectators);

            Start();
        }

        public void Start(int delay = 0) {
            if (Room.GameCompleted(delay)) return;

            State = GameState.Bidding;

            foreach (Player player in Players) 
                player.ClearCalls();

            Table = null;

            Dealer = Dealer.Next;
            Current = Dealer.Next;

            Card.Distribute(Players);

            foreach (Player player in Players) {
                player.ClearRecords();
                player.SendMessage(delay, "GameStarted", Array.IndexOf(Players, Dealer), player.Cards.ToStr());
            }

            Spectators.ClearRecords();
            Spectators.SendMessage(delay, "GameStarted", Array.IndexOf(Players, Dealer), Players.SelectMany(i => i.Cards.Concat(i.Talon)).ToList().ToStr());
        }

        public void Bid(Player sender, Suit? suit) {
            if (State != GameState.Bidding || sender != Current) return;

            if (suit == null) {
                if (Current == Dealer) return;

                Current.RevealTalon();
                Current = Current.Next;

                Broadcast("TrumpNext");

            } else {
                Table = new Table(suit.Value);
                History.Add(new Round(Current, suit.Value));

                Broadcast("TrumpChosen", suit.ToString());
                sender.User.Bidded++;

                do {
                    Current.RevealTalon();
                    Current = Current.Next;

                } while (Current != Dealer.Next);

                State++;
                Current = Dealer.Next;
            }
        }

        public void TalonBid(Player sender, int index) {
            if (State != GameState.Bidding || sender != Current || sender != Dealer) return;

            Broadcast("TalonChosen", (int)sender.Talon[index]);

            Bid(sender, sender.Talon[index].Suit);
        }

        public bool? Declare(Player player, List<int> indexes) {
            if (State != GameState.Declaring || player != Current)
                return null;

            if (indexes?.Any(i => i < 0 || 8 <= i) == true) return false;

            if (!Current.CreateCalls(indexes)) return false;

            if (Current == Dealer) {
                Player maxPlayer = Players.RotateWith(i => i == Dealer.Next).Aggregate((a, b) => a.Calls.Gt(b.Calls)? a : b);
                int total = History.Last().ApplyCalls(maxPlayer);

                int delay = total != 0? 1500 : 0;
                Broadcast(delay, "WinningDeclaration", Array.IndexOf(Players, maxPlayer), total, maxPlayer.Calls.ToString(), maxPlayer.Teammate.Calls.ToString());
                
                maxPlayer.User.Calls20 += maxPlayer.Calls.IndividualCalls.Count(i => i.Value == 20);
                maxPlayer.User.Calls50 += maxPlayer.Calls.IndividualCalls.Count(i => i.Value == 50);
                maxPlayer.User.Calls100 += maxPlayer.Calls.IndividualCalls.Count(i => i.Value == 100);
                maxPlayer.User.Calls150 += maxPlayer.Calls.IndividualCalls.Count(i => i.Value == 150);
                maxPlayer.User.Calls200 += maxPlayer.Calls.IndividualCalls.Count(i => i.Value == 200);
                maxPlayer.User.SixRow += maxPlayer.Calls.IndividualCalls.Count(i => i.Cards.Count == 6);
                maxPlayer.User.SevenRow += maxPlayer.Calls.IndividualCalls.Count(i => i.Cards.Count == 7);

                maxPlayer.Teammate.User.Calls20 += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Value == 20);
                maxPlayer.Teammate.User.Calls50 += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Value == 50);
                maxPlayer.Teammate.User.Calls100 += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Value == 100);
                maxPlayer.Teammate.User.Calls150 += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Value == 150);
                maxPlayer.Teammate.User.Calls200 += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Value == 200);
                maxPlayer.Teammate.User.SixRow += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Cards.Count == 6);
                maxPlayer.Teammate.User.SevenRow += maxPlayer.Teammate.Calls.IndividualCalls.Count(i => i.Cards.Count == 7);

                if (maxPlayer.Calls.IsBelot || maxPlayer.Teammate.Calls.IsBelot) {
                    if (maxPlayer.Calls.IsBelot) maxPlayer.User.Belotes++;
                    if (maxPlayer.Teammate.Calls.IsBelot) maxPlayer.Teammate.User.Belotes++;

                    Room.BelotCompleted(maxPlayer.Team);

                } else Broadcast(delay + maxPlayer.DeclarationDelay(), "StartPlayingCards");

                State++;
            }

            Broadcast("PlayerDeclared", Current.Calls.Max.Value);

            Current = Current.Next;
            return true;
        }

        public int PlayCard(Player player, int card) {
            if (State != GameState.Playing || player != Current)
                return -1;

            if (card < 0 || Current.Cards.Count <= card) return -1;

            if (!Table.Play(Current, card, out bool bela, out Card played)) return -1;

            if (bela) History.Last().Bela(Current);

            if (Table.Complete()) {
                bool last = Players[0].Cards.Count == 0;
                Round current = History.Last();

                current.Play(Table, last, out Current);

                string roundPts = current.ToStringNoFail();
                string roundHidden = current.ToStringHidden();

                if (Room.Type == GameType.Dosta && !last) {
                    int[] score = Score;

                    for (int i = 0; i < 2; i++) {
                        score[i] += current.Played[i] + (current.Played[i] > 0? current.Calls[i] : 0);
                        if (score[i] >= Room.ScoreGoal) {
                            last = true;
                            break;
                        }
                    }
                }
                
                if (last) {
                    current.Finish(last);
                    
                    foreach (Player i in Players) {
                        i.User.PointsScored += current.Played[player.Team];
                        i.User.MaxPointsRound = Math.Max(i.User.MaxPointsRound, current.Played[player.Team]);
                    }

                    if (current.Fail) {
                        current.Bidder.User.BidFailures++;
                    } else {
                        current.Bidder.User.BidSuccesses++;
                    }

                    Start(3000);

                    for (int i = 0; i < 4; i++)
                        Broadcast("FinalCards", i, Players[i].OriginalCards.ToStr(), Players[i].OriginalTalon.ToStr());

                    Broadcast(2000, "FinalScores", $"{current};{Array.IndexOf(Players, Current)}");

                    Broadcast(3000, "TotalScore", current.ToString(), Score.ToStr());
                
                } else Broadcast(2000, "ContinuePlayingCards", Array.IndexOf(Players, Current));
                
                Broadcast("TableComplete", $"{(last? roundPts : (Room.ShowPts? roundPts : roundHidden))},{current.Suit},{current.Fail}");

            } else Current = Current.Next;

            Broadcast("CardPlayed", (int)played, bela);

            return card;
        }

        public void Bela(Player player, bool bela) {
            if (State != GameState.Playing || player != Current)
                return;

            Table.Bela?.TrySetResult(bela);
        }

        Message ReconnectMessage => new Message("Reconnect", Room.ToString(), History.Where(i => i.Finalized).ToStr(), (AlebClient.TimeNow - StartTime).TotalMilliseconds);

        public void Reconnect(Player player) {
            player.User.Client.Send(ReconnectMessage);
            player.User.Client.Flush();

            player.ReplayRecords();
        }

        public void Spectate(User user) {
            user.Client.Send(ReconnectMessage);
            user.Client.Flush();

            Spectators.ReplayRecords(user);
        }
    }
}
