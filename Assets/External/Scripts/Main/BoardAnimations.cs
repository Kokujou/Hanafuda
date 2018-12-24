using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        private Card[] Hovered;
        private void HoverCards(params Card[] cards)
        {
            for (int card = 0; card < Hovered.Length; card++)
                Hovered[card]?.HoverCard(true);
            for (int card = 0; card < cards.Length; card++)
                cards[card]?.HoverCard();
            Hovered = cards;
        }

        private void HoverMatches(Card.Months month)
        {
            for (int card = 0; card < Field.Count; card++)
            {
                if (month == Card.Months.Null)
                    Field[card].FadeCard(false);
                else
                    Field[card].FadeCard(month != Field[card].Monat);
            }
        }
    }
}
