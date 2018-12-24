
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        public void HoverHand(Card card)
        {
            if (card)
            {
                HoverCards(card);
                HoverMatches(card.Monat);
            }
            else
            {
                HoverCards();
                HoverMatches(Card.Months.Null);
            }
        }
        public void SelectCard(Card card)
        {

        }
    }
}
