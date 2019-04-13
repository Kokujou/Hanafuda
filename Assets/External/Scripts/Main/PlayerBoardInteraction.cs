
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

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
                    for (int match = 0; match < matches.Count; match++)
                    {
                        if (matches[match] != card)
                            matches[match].FadeCard(true);
                    }
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
            bool turn = Turn;

            if (Settings.Multiplayer && turn)
            {
                currentAction.SayKoikoi(koikoi);
                OpponentTurn();
            }
            else if (turn && koikoi)
                OpponentTurn();

            if (!koikoi)
            {
                Settings.Players[0].tempPoints = 0;
                Settings.Players[1].tempPoints = 0;
                if (turn)
                {
                    /*
                     * Win-Animation
                     */
                }
                else
                {
                    /*
                     * Loose-Animation
                     */
                }
                SceneManager.LoadScene("Finish");
            }
            else
            {
                /*
                 * Koikoi-Behandlung für Spieler bzw. Gegner
                 */
                if (turn) ;
                else;
            }
        }

        public void OpponentTurn()
        {
            _Turn = false;
            if (!Settings.Multiplayer)
            {
                Move move = ((KI)players[1 - Settings.PlayerID]).MakeTurn(new VirtualBoard(this));
                move.PlayerID = 1 - Settings.PlayerID;
                ApplyMove(move);
            }
            else
            {
                PlayerInteraction.SendAction(currentAction);
            }
        }

        private void ApplyMove(Move move)
        {
            if (move.PlayerID == Settings.PlayerID) return;
            PlayerAction action = PlayerAction.FromMove(move, this);
            action.PlayerID = move.PlayerID;
            Debug.Log(action.ToString());
            AnimateAction(action);
            currentAction = new PlayerAction();
            currentAction.Init(this);
        }

        public void SelectCard(Card card, bool fromDeck = false)
        {
            List<Card> Source = fromDeck ? Deck : players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID].Hand;
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

            List<Action> animationQueue = new List<Action>();
            animationQueue.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));
            if (!fromDeck)
            {
                animationQueue.Add(() => StartCoroutine(players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID].Hand.ResortCards(new CardLayout(true))));
                animationQueue.Add(() => SelectCard(Deck[0], true));
            }
            else if (Collection.Count > 1)
                animationQueue.Add(() =>
                {
                    List<Yaku> NewYaku = Yaku.GetNewYakus(players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID], Collection);
                    if (NewYaku.Count > 0)
                        Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>().Init(NewYaku, this);
                    else
                        OpponentTurn();
                });
            else animationQueue.Add(OpponentTurn);

            StartCoroutine(Animations.CoordinateQueue(animationQueue));
        }
        public void OnGUI()
        {
            if (GUILayout.Button("Cheat Player"))
            {
                players[Settings.PlayerID].CollectedCards = new List<Card>(Global.allCards);
            }
            if (GUILayout.Button("Cheat Opp."))
                players[1 - Settings.PlayerID].CollectedCards = new List<Card>(Global.allCards);
        }
    }
}
