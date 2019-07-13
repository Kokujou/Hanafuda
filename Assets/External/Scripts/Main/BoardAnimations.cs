
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;
using Hanafuda.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class Spielfeld : ISpielfeld
    {
        protected override void HoverCards(params ICard[] cards)
        {
            for (int card = 0; card < Hovered.Length; card++)
                Hovered[card]?.HoverCard(true);
            for (int card = 0; card < cards.Length; card++)
                cards[card]?.HoverCard();
            Hovered = cards;
        }

        protected override void HoverMatches(Months month)
        {
            for (int card = 0; card < Field.Count; card++)
            {
                if (month == Months.Null)
                    Field[card].FadeCard(false);
                else
                    Field[card].FadeCard(month != Field[card].Month);
            }
        }

        protected override void SelectionToField(ICard card)
        {
            Field.Add(card);
            card.GetObject().transform.SetParent(Field3D.transform);
            float scaleFactor = Settings.Mobile ? 1.5f : 1;
            int maxSize = Settings.Mobile ? 3 : 2;
            float offsetX = Animations.StandardScale.x / scaleFactor;
            float offsetY = Animations.StandardScale.y / scaleFactor;
            float cardWidth = Animations._CardSize * offsetX;
            float cardHeight = Animations._CardSize * offsetY;
            float alignY = (cardHeight + offsetY) * (maxSize - 1) * 0.5f;
            Vector3 FieldPos = new Vector3((Field.Count / maxSize) * (cardWidth + offsetX), -alignY + (Field.Count % maxSize) * (cardHeight + offsetY), 0);
            StartCoroutine(card.GetObject().transform.StandardAnimation(Field3D.position + FieldPos, new Vector3(0, 180, 0),
                Animations.StandardScale / scaleFactor));
        }

        public override void CollectCards(List<ICard> ToCollect)
        {
            TurnCollection.AddRange(ToCollect);
            HoverMatches(Months.Null);
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
                    CardMotive type = ToCollect[card].Motive;
                    parent = MainSceneVariables.boardTransforms.PCCollections[(Turn ? 0 : 1) * 4 + (int)type];
                    int inCollection = parent.GetComponentsInChildren<BoxCollider>().Length;
                    Vector3 insertPos = new Vector3((Animations._CardSize / 2f) * (int)(inCollection % 5), -(int)(inCollection / 5) * 5, -inCollection);
                    ToCollect[card].GetObject().transform.parent = parent;
                    destPos = parent.position + insertPos;

                    ToCollect[card].GetObject().layer = 0;
                }
                StartCoroutine(ToCollect[card].GetObject().transform.StandardAnimation(destPos, destRot, destScale));
                if (Field.Remove(ToCollect[card]))
                    ((Player)Players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID]).CollectedCards.Add(ToCollect[card]);

            }
            InfoUI.GetYakuList(Turn ? Settings.PlayerID : 1 - Settings.PlayerID).AddCards(ToCollect);
            ToCollect.Clear();
            HoverHand(null);
        }

        protected override void DrawFromDeck()
        {
            Collection.Add(Deck[0]);
        }

        public override void AnimateAction(PlayerAction action)
        {
            List<Action> actions = new List<Action>();
            actions.Add(() => SelectionToField(action.HandSelection));
            actions.Add(() => Players[action.PlayerID].Hand.Remove(action.HandSelection));
            actions.Add(() => StartCoroutine(Players[action.PlayerID].Hand.ResortCards(new CardLayout(true))));

            actions.Add(() =>
            {
                List<ICard> HandCollection;
                if (action.HandFieldSelection != null)
                    HandCollection = new List<ICard>() { action.HandFieldSelection, action.HandSelection };
                else if (Field.Count(x => x.Month == action.HandSelection.Month) == 3) throw new ArgumentException("Es wird versucht zwei identische Karten einzusammeln");
                else
                    HandCollection = new List<ICard>(Field.FindAll(x => x.Month == action.HandSelection.Month));

                if (HandCollection.Count != 1)
                {
                    CollectCards(HandCollection);
                }
            });
            actions.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));

            actions.Add(() => SelectionToField(action.DeckSelection));
            actions.Add(() => Deck.Remove(action.DeckSelection));

            actions.Add(() =>
            {
                List<ICard> DeckCollection;
                if (action.DeckFieldSelection != null)
                    DeckCollection = new List<ICard>() { action.DeckFieldSelection, action.DeckSelection };
                else if (Field.Count(x => x.Month == action.DeckSelection.Month) == 3) throw new ArgumentException("Es wird versucht zwei identische Karten einzusammeln");
                else
                    DeckCollection = new List<ICard>(Field.FindAll(x => x.Month == action.DeckSelection.Month));
                if (DeckCollection.Count != 1)
                {
                    CollectCards(DeckCollection);
                }
            });

            if (action.HadYaku)
            {
                actions.Add(() =>
                {
                    Dictionary<int, int> collectedYaku = Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0);
                    YakuMethods.GetNewYakus(collectedYaku, Players[1 - Settings.PlayerID].CollectedCards, true);
                    Players[1 - Settings.PlayerID].CollectedYaku = collectedYaku;
                    SayKoiKoi(action.Koikoi);
                });
            }

            actions.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));

            actions.Add(() =>
            {
                TurnCollection.Clear();
                Turn = !Turn;
                PlayerComponent player = gameObject.GetComponent<PlayerComponent>();
                if (player)
                    player.Reset();
                Debug.Log($"Opponent Turn Finished.");
            });
            StartCoroutine(Animations.CoordinateQueue(actions));
        }
    }
}
