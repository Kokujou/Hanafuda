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

        public void SelectionToField(Card card)
        {
            card.Object.transform.parent = Field3D.transform;
            float scaleFactor = Settings.Mobile ? 1.5f : 1;
            int maxSize = Settings.Mobile ? 3 : 2;
            float offsetX = Animations.StandardScale.x / scaleFactor;
            float offsetY = Animations.StandardScale.y / scaleFactor;
            float cardWidth = Animations.CardSize * offsetX;
            float cardHeight = Animations.CardSize * offsetY;
            float alignY = (cardHeight + offsetY) * (maxSize - 1) * 0.5f;
            Vector3 FieldPos = new Vector3((Field.Count / maxSize) * (cardWidth + offsetX), -alignY + (Field.Count % maxSize) * (cardHeight + offsetY), 0);
            StartCoroutine(card.Object.transform.StandardAnimation(Field3D.position + FieldPos, new Vector3(0, 180, 0),
                Animations.StandardScale / scaleFactor));
            Field.Add(card);
        }

        public void CollectCards(List<Card> ToCollect)
        {
            HoverMatches(Card.Months.Null);
            Vector3 destPos = Vector3.zero;
            Vector3 destRot = Vector3.zero;
            Vector3 destScale = Vector3.zero;
            if (Settings.Mobile)
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
                Transform parent = null;
                if (!Settings.Mobile)
                {
                    parent = MainSceneVariables.variableCollection.PCCollections[(Turn ? 0 : 1) * 4 + (int)ToCollect[card].Typ];
                    int inCollection = ((Player)players[Turn ? 0 : 1]).CollectedCards.FindAll(x => x.Typ == ToCollect[card].Typ).Count;
                    Vector3 insertPos = new Vector3((Animations.CardSize / 2) * (inCollection % 5), -(inCollection / 5) * 2f, 0);
                    destPos = parent.position + insertPos;
                    ToCollect[card].Object.transform.parent = parent;
                }
                StartCoroutine(ToCollect[card].Object.transform.StandardAnimation(destPos, destRot, destScale));
                ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(ToCollect[card]);
                Field.Remove(ToCollect[card]);
            }
            ToCollect.Clear();
            HoverHand(null);
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
            Collection.Add(Deck[0]);
        }
    }
}
