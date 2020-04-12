using System;
using System.Collections.Generic;
using System.Linq;

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
        public readonly Player Bidder;

        List<Action> played;
        public Action Winner => played.Aggregate((a, b) => a.Card.Gt(b.Card, Trump)? a : b);

        public int Points => played.Sum(i => i.Card.Points(Trump));

        public readonly List<Card> BelaCards;

        public void Clear() => played = new List<Action>();

        public bool Complete() => played.Count == 4;

        public bool Play(Player player, int index, bool bela) {
            if (Complete()) return false;

            Card card = player.Cards[index];

            if (played.Count != 0) {
                IEnumerable<Card> matching = player.Cards.Where(i => i.Suit == played[0].Card.Suit);
                if (!matching.Any()) matching = player.Cards.Where(i => i.Suit == Trump);
                if (!matching.Any()) matching = player.Cards;

                IEnumerable<Card> playable = matching.Where(i => i.Gt(Winner.Card, Trump));
                if (playable.Any() && !playable.Contains(card)) return false;
            }

            if (bela && (BelaCards.Intersect(player.Cards).Count() != BelaCards.Count() || !BelaCards.Contains(card)))
                return false;

            played.Add(new Action(player, card));
            player.Cards.RemoveAt(index);
            return true;
        }

        public Table(Suit trump, Player bidder) {
            Trump = trump;
            Bidder = bidder;

            BelaCards = new List<Card> {
                new Card(Trump, Value.Queen),
                new Card(Trump, Value.King)
            };

            Clear();
        }
    }
}
