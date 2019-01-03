
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        private List<Card> Collection;
        private PlayerAction currentAction;
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

        private bool HandleMatches(Card card, bool fromDeck = false)
        {
            List<Card> matches = Field.FindAll(x => x.Monat == card.Monat);
            switch (matches.Count)
            {
                case 2:
                case 4:
                    Collection = matches;
                    StartCoroutine(Animations.AfterAnimation(() => CollectCards(Collection)));
                    break;
                case 3:
                    card.FadeCard();
                    gameObject.GetComponent<PlayerComponent>().RequestFieldSelection(card, fromDeck);
                    return false;
                default:
                    Collection.Clear();
                    HoverMatches(Card.Months.Null);
                    break;
            }
            return true;
        }
        public void SayKoiKoi(bool koikoi)
        {
            if (Turn)
            {
                currentAction.SayKoikoi(koikoi);
                if (Settings.Multiplayer)
                    OpponentTurn();
            }
            else
            {
                if (koikoi)
                {
                    /*
                     * Koikoi-Animation Einblenden
                     */
                }
            }
            if ((!Settings.Multiplayer || !Turn) && !koikoi)
                SceneManager.LoadScene("Finish");
        }

        public void OpponentTurn()
        {
            _Turn = !_Turn;
            if (!Settings.Multiplayer)
            {
                PlayerAction action = ((KI)players[1 - Settings.PlayerID]).MakeTurn(this);
                gameObject.GetComponent<PlayerComponent>().Reset();
                currentAction = new PlayerAction();
                currentAction.Init(this);
            }
            else
            {
                PlayerInteraction.SendAction(currentAction);
            }
        }

        private void ApplyMove(Move move)
        {
            if (move.PlayerID == Settings.PlayerID) return;
            PlayerAction action = move;
            action.PlayerID = move.PlayerID;
            AnimateAction(action);
            currentAction = new PlayerAction();
            currentAction.Init(this);
        }

        public void SelectCard(Card card, bool fromDeck = false)
        {
            List<Card> Source = fromDeck ? Deck : ((Player)players[Turn ? 0 : 1]).Hand;
            Collection.Add(card);
            // = Erster Aufruf
            if (Collection.Count == 1)
            {
                if (fromDeck)
                    currentAction.DrawCard();
                else
                    currentAction.SelectFromHand(card);
                SelectionToField(card);
                Source.Remove(card);
                if (!HandleMatches(card, fromDeck)) return;
            }
            else
            {
                if (fromDeck)
                    currentAction.SelectDeckMatch(card);
                else
                    currentAction.SelectHandMatch(card);
                StartCoroutine(Animations.AfterAnimation(() => CollectCards(Collection)));
            }
            HoverMatches(Card.Months.Null);
            if (!fromDeck)
            {
                StartCoroutine(Animations.AfterAnimation(() =>
                {
                    Global.MovingCards++;
                    StartCoroutine(((Player)players[Turn ? 0 : 1]).Hand.ResortCards(new CardLayout(true)));
                    StartCoroutine(Field.ResortCards(new CardLayout(false)));
                    StartCoroutine(Animations.AfterAnimation(() => { SelectCard(Deck[0], true); }));
                    Global.MovingCards--;
                }));
            }
            else
            {
                StartCoroutine(Animations.AfterAnimation(CheckNewYaku));
            }
        }
        public void OnGUI()
        {
            if (GUILayout.Button("X"))
                ((Player)players[Settings.PlayerID]).CollectedCards = Global.allCards;
        }
    }
}
