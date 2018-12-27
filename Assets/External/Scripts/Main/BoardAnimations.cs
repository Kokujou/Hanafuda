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
            Vector3 destPos = Vector3.zero;
            Vector3 destRot = Vector3.zero;
            Vector3 destScale = Vector3.zero;
            if (Global.Settings.mobile)
            {
                destPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Turn ? 0 : Screen.height));
            }
            else
            {
                destRot = new Vector3(0, 180, 0);
                destScale = Animations.StandardScale / 2f;
            }
            for (int card = 0; card < ToCollect.Count; card++)
            {
                Debug.Log(ToCollect[card]);
                Transform parent = null;
                if (!Global.Settings.mobile)
                {
                    parent = MainSceneVariables.variableCollection.PCCollections[(Turn ? 0 : 1) * 4 + (int)ToCollect[card].Typ];
                    int inCollection = ((Player)players[Turn ? 0 : 1]).CollectedCards.FindAll(x => x.Typ == ToCollect[card].Typ).Count;
                    Vector3 insertPos = new Vector3((Animations.CardSize / 2) * (inCollection % 5), -(inCollection / 5) * 2f, 0);
                    destPos = parent.position + insertPos;
                    ToCollect[card].Object.transform.parent = parent;
                }
                Debug.Log(destScale);
                StartCoroutine(ToCollect[card].Object.transform.StandardAnimation(destPos, destRot, destScale));
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
