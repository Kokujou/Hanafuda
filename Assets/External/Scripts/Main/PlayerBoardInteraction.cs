
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        private List<Card> ToCollect;
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

        private void SelectionToField(Card card, List<Card> source)
        {
            card.Object.transform.parent = Field3D.transform;
            float scaleFactor = Global.Settings.mobile ? 1.5f : 1;
            int maxSize = Global.Settings.mobile ? 3 : 2;
            float offsetX = Animations.StandardScale.x / scaleFactor;
            float offsetY = Animations.StandardScale.y / scaleFactor;
            float cardWidth = Animations.CardSize * offsetX;
            float cardHeight = Animations.CardSize * offsetY;
            float alignY = (cardHeight + offsetY) * (maxSize - 1) * 0.5f;
            Vector3 FieldPos = new Vector3((Field.Count / maxSize) * (cardWidth + offsetX), -alignY + (Field.Count % maxSize) * (cardHeight + offsetY), 0);
            StartCoroutine(card.Object.transform.StandardAnimation(Field3D.position + FieldPos, new Vector3(0, 180, 0),
                Animations.StandardScale / scaleFactor));
            Field.Add(card);
            source.Remove(card);
        }
        private bool HandleMatches(Card card, bool fromDeck = false)
        {
            List<Card> matches = Field.FindAll(x => x.Monat == card.Monat);
            switch (matches.Count)
            {
                case 2:
                case 4:
                    ToCollect = matches;
                    StartCoroutine(AfterAnimation(CollectCards));
                    break;
                case 3:
                    card.FadeCard();
                    gameObject.GetComponent<PlayerComponent>().RequestFieldSelection(card, fromDeck);
                    return false;
                default:
                    ToCollect.Clear();
                    HoverMatches(Card.Months.Null);
                    break;
            }
            return true;
        }
        public void OpponentTurn()
        {
            _Turn = !_Turn;
            if (!Global.Settings.Multiplayer)
            {
                PlayerAction action = ((KI)players[1]).MakeTurn(this);
                gameObject.GetComponent<PlayerComponent>().Reset();
            }
        }
        public void SelectCard(Card card, bool fromDeck = false)
        {
            List<Card> Source = fromDeck ? Deck : ((Player)players[Turn ? 0 : 1]).Hand;
            ToCollect.Add(card);
            if (ToCollect.Count == 1)
            {
                SelectionToField(card, Source);
                if (!HandleMatches(card, fromDeck)) return;
            }
            else
            {
                StartCoroutine(AfterAnimation(CollectCards));
            }
            HoverMatches(Card.Months.Null);
            if (!fromDeck)
            {
                StartCoroutine(AfterAnimation(() =>
                {
                    Global.MovingCards++;
                    if (Global.Settings.mobile)
                    {
                        StartCoroutine(((Player)players[Turn ? 0 : 1]).Hand.ResortCards(8, true));
                        StartCoroutine(Field.ResortCards(3, rowWise: false));
                    }
                    else
                    {
                        StartCoroutine(((Player)players[Turn ? 0 : 1]).Hand.ResortCards(1, rowWise: false));
                        StartCoroutine(Field.ResortCards(2, rowWise: false));
                    }
                    StartCoroutine(AfterAnimation(() => { SelectCard(Deck[0], true); }));
                    Global.MovingCards--;
                }));
            }
            else
            {
                StartCoroutine(AfterAnimation(CheckNewYaku));
                /*if (NewYaku.Count == 0)
                {
                    _Turn = !_Turn;
                    PlayMode = 1;
                    if (Global.Settings.Multiplayer)
                    {
                        string move = Move[0].ToString() + "," + Move[1].ToString() + "," + Move[2].ToString();
                        if (NetworkServer.active)
                            NetworkServer.SendToAll(MoveSyncMsg, new Message() { message = move });
                        else
                            Global.Settings.playerClients[0].Send(MoveSyncMsg, new Message() { message = move });
                    }
                }
                Move = new[] { -1, -1, -1 };*/
                //StartCoroutine(AfterAnimation(OpponentTurn));
            }
        }
        public void OnGUI()
        {
            if (GUILayout.Button("X"))
                ((Player)players[0]).CollectedCards = Global.allCards;
        }
    }
}
