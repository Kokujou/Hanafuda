
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
            card.Objekt.transform.parent = Field3D.transform;
            float offsetX = card.Objekt.transform.localScale.x / 1.5f;
            float offsetY = card.Objekt.transform.localScale.y / 1.5f;
            float cardWidth = Animations.CardSize * offsetX;
            float cardHeight = Animations.CardSize * offsetY;
            float alignY = (cardHeight + offsetY) * 1f;
            Vector3 FieldPos = new Vector3((Field.Count / 3) * (cardWidth + offsetX), -alignY + (Field.Count % 3) * (cardHeight + offsetY), 0);
            StartCoroutine(card.Objekt.transform.StandardAnimation(Field3D.position + FieldPos, new Vector3(0, 180, 0),
                card.Objekt.transform.localScale / 1.5f));
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
                    ToCollect.AddRange(matches);
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
        private void OpponentTurn()
        {
            _Turn = !false;
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
                    StartCoroutine(((Player)players[Turn ? 0 : 1]).Hand.ResortCards(8, true));
                    StartCoroutine(Field.ResortCards(3, rowWise: false));
                    StartCoroutine(AfterAnimation(() => { SelectCard(Deck[0], true); }));
                    Global.MovingCards--;
                }));
            }
            else
                OpponentTurn();
        }
    }
}
