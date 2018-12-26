using ExtensionMethods;
using System;
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

        private void CollectCards()
        {
            HoverMatches(Card.Months.Null);
            for (int card = 0; card < ToCollect.Count; card++)
            {
                StartCoroutine(ToCollect[card].Objekt.transform.StandardAnimation(
                    Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Turn ? 0 : Screen.height)),
                    Vector3.zero, Vector3.zero));
                ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(ToCollect[card]);
                Field.Remove(ToCollect[card]);
            }
            ToCollect.Clear();
        }

        private IEnumerator AfterAnimation(Action action)
        {
            while (Global.MovingCards > 0)
            {
                //Debug.Log(Global.MovingCards);
                yield return null;
            }
            action();
        }

        private void DrawFromDeck()
        {
            ToCollect.Add(Deck[0]);
        }
    }
}
