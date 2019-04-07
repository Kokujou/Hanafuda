using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            float cardWidth = Animations._CardSize * offsetX;
            float cardHeight = Animations._CardSize * offsetY;
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
                destScale = Animations.StandardScale;
            }
            for (int card = 0; card < ToCollect.Count; card++)
            {
                Transform parent = null;
                if (!Settings.Mobile)
                {
                    Card.Type type = ToCollect[card].Typ;
                    parent = MainSceneVariables.variableCollection.PCCollections[(Turn ? 0 : 1) * 4 + (int)type];
                    int inCollection = parent.GetComponentsInChildren<BoxCollider>().Length;
                    Vector3 insertPos = new Vector3((Animations._CardSize / 2f) * (int)(inCollection % 5), -(int)(inCollection / 5) * 5, -inCollection);
                    ToCollect[card].Object.transform.parent = parent;
                    destPos = parent.position + insertPos;

                    ToCollect[card].Object.layer = 0;
                }
                StartCoroutine(ToCollect[card].Object.transform.StandardAnimation(destPos, destRot, destScale));
                ((Player)players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID]).CollectedCards.Add(ToCollect[card]);
                Field.Remove(ToCollect[card]);
            }
            InfoUI.GetYakuList(Turn ? Settings.PlayerID : 1 - Settings.PlayerID).AddCards(ToCollect);
            ToCollect.Clear();
            HoverHand(null);
        }

        private void DrawFromDeck()
        {
            Collection.Add(Deck[0]);
        }

        public void AnimateAction(PlayerAction action)
        {
            List<Action> actions = new List<Action>();
            actions.Add(() => SelectionToField(action.HandSelection));
            actions.Add(() => players[action.PlayerID].Hand.Remove(action.HandSelection));
            actions.Add(() => StartCoroutine(players[action.PlayerID].Hand.ResortCards(new CardLayout(true))));

            actions.Add(() =>
            {
                List<Card> HandCollection;
                if (action.HandFieldSelection)
                    HandCollection = new List<Card>() { action.HandFieldSelection, action.HandSelection };
                else
                    HandCollection = new List<Card>(Field.FindAll(x => x.Monat == action.HandSelection.Monat));

                if (HandCollection.Count != 1)
                {
                    Field.RemoveAll(x => HandCollection.Contains(x));
                    CollectCards(HandCollection);
                }
            });
            actions.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));

            actions.Add(() => SelectionToField(action.DeckSelection));
            actions.Add(() => Deck.Remove(action.DeckSelection));

            actions.Add(() =>
            {
                List<Card> DeckCollection;
                if (action.DeckFieldSelection)
                    DeckCollection = new List<Card>() { action.DeckFieldSelection, action.DeckSelection };
                else
                    DeckCollection = new List<Card>(Field.FindAll(x => x.Monat == action.DeckSelection.Monat));
                if (DeckCollection.Count != 1)
                {
                    Field.RemoveAll(x => DeckCollection.Contains(x));
                    CollectCards(DeckCollection);
                }
            });

            if (action.HadYaku)
            {
                if (action.Koikoi)
                    players[action.PlayerID].Koikoi++;
                actions.Add(() => SayKoiKoi(action.Koikoi));
            }

            actions.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));

            actions.Add(() =>
            {
                Turn = !Turn; gameObject.GetComponent<PlayerComponent>().Reset();
                Debug.Log($"Opponent Turn Finished.");
            });
            StartCoroutine(Animations.CoordinateQueue(actions));
        }
    }
}
