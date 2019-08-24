using System.Collections.Generic;
using System.Linq;

namespace Hanafuda
{
    struct VirtualBoard : IHanafudaBoard
    {
        public List<Card> Deck { get; set; }
        public List<Card> Field { get; set; }
        public List<Player> Players { get; set; }
        public bool Turn { get; set; }

        public void ApplyMove(Move move)
        {
            Player activePlayer = Players[move.PlayerID];
            Card handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);
            activePlayer.Hand.Remove(handSelection);

            List<Card> handMatches = Field.FindAll(x => x.Monat == handSelection.Monat);
            if (handMatches.Count == 2)
                handMatches = new List<Card>() { handMatches.First(x => x.Title == move.HandFieldSelection) };
            else if (handMatches.Count == 0)
                Field.Add(handSelection);
            else
                handMatches.Add(handSelection);
            activePlayer.CollectedCards.AddRange(handMatches);
            foreach (Card card in handMatches)
                Field.Remove(card);

            Card deckSelection = Deck[0];
            Deck.RemoveAt(0);

            List<Card> deckMatches = Field.FindAll(x => x.Monat == deckSelection.Monat);
            if (deckMatches.Count == 2)
                deckMatches = new List<Card>() { deckMatches.First(x => x.Title == move.DeckFieldSelection) };
            else if (deckMatches.Count == 0)
                Field.Add(deckSelection);
            else
                deckMatches.Add(deckSelection);
            activePlayer.CollectedCards.AddRange(deckMatches);
            foreach (Card card in deckMatches)
                Field.Remove(card);
        }
    }
}