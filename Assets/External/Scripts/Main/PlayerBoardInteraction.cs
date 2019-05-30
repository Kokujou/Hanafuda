
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

namespace Hanafuda
{
    public partial class Spielfeld : ISpielfeld
    {
        public override void HoverHand(Card card)
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

        protected override void HandleMatches(Card card, List<Action> AnimationQueue, bool fromDeck = false)
        {
            List<Card> matches = Field.FindAll(x => x.Monat == card.Monat);
            switch (matches.Count)
            {
                case 2:
                case 4:
                    Collection = matches;
                    CollectCards(Collection);
                    break;
                case 3:
                    HoverMatches(card.Monat);
                    card.FadeCard();
                    gameObject.GetComponent<PlayerComponent>().RequestFieldSelection(card, fromDeck);
                    StartCoroutine(WaitForFieldSelection(fromDeck));
                    break;
                default:
                    Collection.Clear();
                    HoverMatches(Card.Months.Null);
                    break;
            }
        }
        public override void SayKoiKoi(bool koikoi)
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
                Player player = players[turn ? Settings.PlayerID : 1 - Settings.PlayerID];
                player.tempPoints = player.CollectedYaku.Sum(x => Global.allYaku[x.Key].GetPoints(x.Value));
                if (player.tempPoints == 0) Debug.Log(string.Join(";", player.CollectedCards));
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
                players[turn ? Settings.PlayerID : 1 - Settings.PlayerID].Koikoi++;
                /*
                 * Koikoi-Behandlung für Spieler bzw. Gegner
                 */
                if (turn) ;
                else;
            }
        }

        public override void DrawnGame()
        {
            /*
             * Drawn Game Animation
             */
            SceneManager.LoadScene("Finish");
        }

        protected override void OpponentTurn()
        {
            Turn = false;
            if (players[1 - Settings.PlayerID].Hand.Count <= 0)
            {
                DrawnGame();
                return;
            }
            if (!Settings.Multiplayer)
            {
                Move move = ((IArtificialIntelligence)players[1 - Settings.PlayerID]).MakeTurn(this);
                move.PlayerID = 1 - Settings.PlayerID;
                ApplyMove(move);
            }
            else
            {
                PlayerInteraction.SendAction(currentAction);
                currentAction = new PlayerAction();
                currentAction.Init(this);
            }
        }

        protected override void ApplyMove(Move move)
        {
            if (move.PlayerID == Settings.PlayerID) return;
            PlayerAction action = PlayerAction.FromMove(move, this);
            action.PlayerID = move.PlayerID;
            Debug.Log(action.ToString());
            AnimateAction(action);
            currentAction = new PlayerAction();
            currentAction.Init(this);
        }

        public override void SelectCard(Card card, bool fromDeck = false)
        {
            List<Action> animationQueue = new List<Action>();
            List<Card> Source = fromDeck ? Deck : players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID].Hand;
            animationQueue.Add(() => Collection.Add(card));
            // = Erster Aufruf
            animationQueue.Add(() =>
            {
                if (fromDeck)
                    currentAction.DrawCard();
                else
                    currentAction.SelectFromHand(card);
                Source.Remove(card);
                SelectionToField(card);
            });

            animationQueue.Add(() => HandleMatches(card, animationQueue, fromDeck));
            animationQueue.Add(() => HoverMatches(Card.Months.Null));

            animationQueue.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));
            animationQueue.Add(() => StartCoroutine(players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID].Hand.ResortCards(new CardLayout(true))));

            animationQueue.Add(() =>
            {
                currentAction.DrawCard();
                SelectionToField(Deck[0]);
            });

            animationQueue.Add(() => HandleMatches(Deck[0], animationQueue, fromDeck));
            animationQueue.Add(() => HoverMatches(Card.Months.Null));
            animationQueue.Add(() => Deck.RemoveAt(0));

            animationQueue.Add(() => StartCoroutine(Field.ResortCards(new CardLayout(false))));

            animationQueue.Add(() =>
            {
                Debug.Log(string.Join(";", players[Settings.PlayerID].CollectedCards));
                List<Yaku> NewYaku = Yaku.GetNewYakus(players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID].CollectedYaku, TurnCollection, true);
                TurnCollection.Clear();
                if (NewYaku.Count > 0)
                    Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>().Init(NewYaku, this);
                else
                    OpponentTurn();
            });

            StartCoroutine(Animations.CoordinateQueue(animationQueue));
        }

        private IEnumerator WaitForFieldSelection(bool fromDeck)
        {
            Global.MovingCards++;
            while (fromDeck ? currentAction.DeckFieldSelection : currentAction.HandFieldSelection == null)
                yield return null;
            Collection.Add(fromDeck ? currentAction.DeckFieldSelection : currentAction.HandFieldSelection);
            CollectCards(Collection);
            Global.MovingCards--;
        }

        private void OnGUI()
        {
#if UNITY_EDITOR
            if (GUILayout.Button("Cheat Player"))
            {
                players[Settings.PlayerID].CollectedCards = new List<Card>(Global.allCards);
                players[Settings.PlayerID].CollectedYaku = Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0);
                Settings.Players = players;
                List<Yaku> NewYaku = Yaku.GetNewYakus(players[Settings.PlayerID].CollectedYaku, players[Settings.PlayerID].CollectedCards, true);
                Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>().Init(new List<Yaku>(Global.allYaku), this);

            }
            if (GUILayout.Button("Cheat Opp."))
                players[1 - Settings.PlayerID].CollectedCards = Global.allCards.FindAll(x => Global.allYaku.First(y => y.Title == "Hanamizake").Contains(x));
            if (GUILayout.Button("Skip to Finish"))
                SceneManager.LoadScene("Finish");
#endif
        }
    }
}
