using Hanafuda.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base
{
    public struct VirtualBoard : IHanafudaBoard
    {
        public List<ICard> Deck { get; set; }
        public List<ICard> Field { get; set; }
        public List<Player> Players { get; set; }
        public bool Turn { get; set; }

        public void ApplyMove(Move move)
        {
            Player activePlayer = Players[move.PlayerID];
            ICard handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);
            activePlayer.Hand.Remove(handSelection);

            List<ICard> handMatches = Field.FindAll(x => x.Month == handSelection.Month);
            if (handMatches.Count == 2)
                handMatches = new List<ICard>() { handMatches.First(x => x.Title == move.HandFieldSelection) };
            else if (handMatches.Count == 0)
                Field.Add(handSelection);
            else
                handMatches.Add(handSelection);
            activePlayer.CollectedCards.AddRange(handMatches);
            foreach (ICard card in handMatches)
                Field.Remove(card);

            ICard deckSelection = Deck[0];
            Deck.RemoveAt(0);

            List<ICard> deckMatches = Field.FindAll(x => x.Month == deckSelection.Month);
            if (deckMatches.Count == 2)
                deckMatches = new List<ICard>() { deckMatches.First(x => x.Title == move.DeckFieldSelection) };
            else if (deckMatches.Count == 0)
                Field.Add(deckSelection);
            else
                deckMatches.Add(deckSelection);
            activePlayer.CollectedCards.AddRange(deckMatches);
            foreach (ICard card in deckMatches)
                Field.Remove(card);

        }
    }
}
