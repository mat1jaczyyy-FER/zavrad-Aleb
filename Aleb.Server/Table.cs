using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aleb.Common;

namespace Aleb.Server {
    class Action {
        public readonly Player Player;
        public readonly Card Card;

        public Action(Player player, Card card) {
            Player = player;
            Card = card;
        }
    }

    class Table {
        Suit Trump;

        public TaskCompletionSource<bool> Bela { get; private set; }

        List<Action> played;
        public Action Winner => played.Aggregate((a, b) => a.Card.Gt(b.Card, Trump, played[0].Card.Suit)? a : b);

        public int Points => played.Sum(i => i.Card.Points(Trump));

        public readonly List<Card> BelaCards;

        public void Clear() => played = new List<Action>();

        public bool Complete() => played.Count == 4;

        public bool Play(Player player, int index, out bool bela, out Card card) {
            bela = false;
            card = player.Cards[index];

            if (Complete()) return false;

            if (played.Count != 0) {
                IEnumerable<Card> matching = player.Cards.Where(i => i.Suit == played[0].Card.Suit);

                if (!matching.Any()) {
                    IEnumerable<Card> trumps = matching = player.Cards.Where(i => i.Suit == Trump);

                    if (matching.Any()) {
                        matching = matching.Where(i => i.Gt(Winner.Card, Trump, played[0].Card.Suit));
                        if (!matching.Any()) matching = trumps;
                    }

                } else {
                    IEnumerable<Card> following = matching;

                    matching = matching.Where(i => i.Gt(Winner.Card, Trump, played[0].Card.Suit));
                    if (!matching.Any()) matching = following;
                }

                if (matching.Any() && !matching.Contains(card)) return false;
            }

            if (BelaCards.Intersect(player.Cards).Count() == BelaCards.Count() && BelaCards.Contains(card)) {
                Bela = new TaskCompletionSource<bool>();

                player.SendMessage("AskBela");
                player.Flush();

                bela = Bela.Task.Result;
                Bela = null;
            }

            played.Add(new Action(player, card));
            player.Cards.RemoveAt(index);
            return true;
        }

        public Table(Suit trump) {
            Trump = trump;

            BelaCards = new List<Card> {
                new Card(Trump, Value.Queen),
                new Card(Trump, Value.King)
            };

            Clear();
        }
    }
}
