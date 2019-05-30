using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class OmniscientAI : KI<OmniscientBoard>
    {
        const string _LocalWeight = "_LocalWeight";
        const string _GlobalWeight = "_GlobalWeight";
        const string _CollectionWeight = "_CollectionWeight";
        const string _PassWeight = "_PassWeight";
        const string _WaitWeight = "_WaitWeight";
        const string _WaitTurnCompensation = "_WaitTurnCompensation";
        const string _OpponentInterestWeight = "_OpponentInterestWeight";
        const string _PassCollectionWeight = "_PassCollectionWeight";

        public int _MaxPTurns = 3;

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _GlobalWeight, 100f },
            { _LocalWeight, 1f },
            { _CollectionWeight, 6f },
            { _PassWeight, 1f },
            {_WaitTurnCompensation, 0.1f },
            {_WaitWeight,6f },
            {_OpponentInterestWeight, 4f },
            { _PassCollectionWeight, 2f }
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        protected override void BuildStateTree(Spielfeld cRoot)
        {
            OmniscientBoard root = new OmniscientBoard(cRoot);
            root.Turn = true;
            Tree = new OmniscientStateTree(new OmniscientBoard(cRoot));
            Tree.Build(1);
        }

        public OmniscientAI(string name) : base(name) { }

        public override float RateState(OmniscientBoard State)
        {
            /*
             *  - Vorberechnung von allen Karten bezüglich Relevanz für die beteiligten Yaku
             *      -> dynamische Wichtung mit den Wahrscheinlichkeiten der Yaku
             *          -> Kartenwahrscheinlichkeit über Pfade
             *      -> Miteinbeziehung der Punktzahlen der Yaku und der Mehrfachen Erreichung des selben Yaku
             *  - Vergleich der Yaku-Erreichung vor dem Gegner als Haupteinfluss
             *      - Nicht Karten sondern Spielzüge, (Kettenregel?)
             *      - Mitberechnung gegnerischer Züge! -> Aufbau des Baums!
             *  - Whkt Idee: Gegnerische Matches abziehen!
             *  - Memo: besserer Näherungswert für Zeit bis Kartenzug
             *  - Endwert: Verfolgung des globalen Minimum + Kombination der lokalen Minima (Summe des prozentualen Fortschritts?)
             *  - Memo: Möglicher Possible-Yaku-Verlust durch Einsammeln
             *  - Hinzufügen: Wertekombination gegnerischer Züge -> verbauen?
             *  - Memo: IsFinal implementieren
             *  - Balancing der verschiedenen Werte
             */
            lock (OmniscientStateTree.thisLock)
                Global.Log($"Zustand {State.GetHashCode()}: {PlayerAction.FromMove(State.LastMove, MainSceneVariables.boardTransforms.Main).ToString().Replace("\n", "")}");
            if (State.isFinal) return Mathf.Infinity;

            float Result = 0f;

            StateProps ComStateProps = RateSingleState(State, true);

            OmniscientStateTree PlayerTree = new OmniscientStateTree(State);
            PlayerTree.Build(1, false, true);
            List<OmniscientBoard> PStates = PlayerTree.GetLevel(1);
            List<StateProps> PStateProps = new List<StateProps>();
            List<Card> NewCards = State.computer.CollectedCards.Except(State.parent.computer.CollectedCards).ToList();

            foreach (OmniscientBoard PState in PStates)
                PStateProps.Add(RateSingleState(PState, false));

            float PGlobalMinimum = PStateProps.Min(x => x.GlobalMinimum.MinTurns * (2f - x.GlobalMinimum.Probability));
            float ComGlobalMinimum = ComStateProps.GlobalMinimum.MinTurns * (2f - ComStateProps.GlobalMinimum.Probability);
            float GlobalValue = PGlobalMinimum -
                ComGlobalMinimum;
            float PLocalMinimum = PStateProps.Max(x => x.LocalMinimum);
            float LocalValue = PLocalMinimum - ComStateProps.LocalMinimum;

            float CollectionValue = NewCards
                .Sum(x => CardProps.First(y => y.card.Title == x.Title)
                .RelevanceForYaku.Sum(z => z.Value));

            float PassValue = 0f;
            Card handSelection = Global.allCards.First(x => x.Title == State.LastMove.HandSelection);
            Card deckSelection = Global.allCards.First(x => x.Title == State.LastMove.DeckSelection);
            List<Card> PassedCards = new List<Card>();
            if (!NewCards.Contains(handSelection))
                PassedCards.Add(handSelection);
            if (!NewCards.Contains(deckSelection))
                PassedCards.Add(deckSelection);

            foreach (Card passedCard in PassedCards)
            {
                float cardPassValue = 0f;

                float WaitValue = GetWaitValue(State, passedCard);
                float OpponentInterst = State.player.Hand.Contains(passedCard) ? -1 : 0;
                float PassCollection = State.computer.Hand.Count(x => x.Monat == passedCard.Monat) >= 2 ? 1 : 0;

                cardPassValue = WaitValue * weights[_WaitWeight]
                    + OpponentInterst * weights[_OpponentInterestWeight]
                    + PassCollection * weights[_PassCollectionWeight];

                Global.Log($"\n\nGepasst auf Karte: {passedCard.Title}\n" +
                    $"Wait Value: {WaitValue}\n" +
                    $"Gegnerisches Interesse: {OpponentInterst}\n" +
                    $"Passen zwecks Einsammeln: {PassCollection}\n" +
                    $"Gesamtwert: {cardPassValue}\n\n");

                PassValue += cardPassValue;
            }


            Result = GlobalValue * weights[_GlobalWeight]
                + LocalValue * weights[_LocalWeight]
                + CollectionValue * weights[_CollectionWeight]
                + PassValue * weights[_PassWeight];

            Global.Log($"Endgültige Werte:\n" +
                $"Globales Minimum (COM): {ComGlobalMinimum}, Wahrscheinlichkeit: {ComStateProps.GlobalMinimum.Probability} Name: {Global.allYaku[ComStateProps.GlobalMinimum.ID].Title}\n" +
                $"Globales Minimum (P): {PGlobalMinimum}\n" +
                $"Lokales Minimum (COM): {ComStateProps.LocalMinimum}\n" +
                $"Lokales Minimum (P): {PLocalMinimum}\n" +
                $"Sammlungswert: {CollectionValue}\n" +
                $"Gepasst auf Karten: {string.Join(";", PassedCards)}" +
                $"Passwert: {PassValue}\n" +
                $"Gesamtwert: {Result}");

            return Result;
        }

        private float GetWaitValue(OmniscientBoard State, Card passedCard)
        {
            Card bestFieldMatch = null;
            float maxFieldValue = 0f;
            foreach (Card handCard in State.computer.Hand)
            {
                if (handCard == passedCard) continue;
                foreach (Card fieldCard in State.Field)
                {
                    if (fieldCard.Monat == handCard.Monat)
                    {
                        float relevanceSum = CardProps.First(y => y.card.Title == fieldCard.Title).RelevanceForYaku.Sum(x => x.Value);
                        if (relevanceSum > maxFieldValue)
                        {
                            maxFieldValue = relevanceSum;
                            bestFieldMatch = handCard;
                        }
                    }
                }
            }

            if (bestFieldMatch == null)
                return 0f;

            float maxDeckValue = 0f;
            int bestDeckTurnID = 0;
            for (int turnID = 0; turnID < State.computer.Hand.Count; turnID++)
            {
                Card deckCard = State.Deck[turnID * 2 + 1];
                if (deckCard.Monat == bestFieldMatch.Monat)
                {
                    float relevanceSum = CardProps.First(y => y.card.Title == deckCard.Title).RelevanceForYaku.Sum(x => x.Value);
                    if (relevanceSum > maxFieldValue)
                    {
                        maxFieldValue = relevanceSum;
                        bestDeckTurnID = turnID;
                    }
                }
            }
            return (maxDeckValue - weights[_WaitTurnCompensation] * bestDeckTurnID) - maxFieldValue;
        }

        public struct StateProps
        {
            public YakuProperties GlobalMinimum;
            public float LocalMinimum;
        }

        public StateProps RateSingleState(OmniscientBoard State, bool Turn)
        {
            Player activePlayer = Turn ? State.computer : State.player;

            YakuProperties GlobalMinimum = null;

            float TotalCardValue = 0;

            List<Card> NewCards = new List<Card>();
            if (State.LastMove != null)
            {
                OmniscientBoard state = State.parent;
                NewCards = activePlayer.CollectedCards.Except((Turn ? state.computer : state.player).CollectedCards).ToList();
            }

            OmniscientCards cardProperties = new OmniscientCards(CardProps, State, Turn);
            YakuCollection OmniscientYakuProps = new YakuCollection(cardProperties, NewCards, activePlayer.CollectedCards, activePlayer.Hand.Count);

            foreach (YakuProperties yakuProp in OmniscientYakuProps.Where(x => x.Probability > 0))
            {
                float value = yakuProp.MinTurns * (2f - yakuProp.Probability);
                if (GlobalMinimum == null || value < GlobalMinimum.MinTurns * (2f - GlobalMinimum.Probability))
                    GlobalMinimum = yakuProp;
            }
            if (GlobalMinimum == null) GlobalMinimum = OmniscientYakuProps.OrderByDescending(x => x.MinTurns).ToArray()[0];

            try
            {
                TotalCardValue = OmniscientYakuProps
                    .Where(x => x.Targeted && x.Probability > 0)
                    .Sum(x => x.MinTurns * (2f - x.Probability));
            }
            catch (Exception) { }

            if (false)
            {
                Global.Log($"{State.GetHashCode()} -> YakuProps: [{string.Join(";", OmniscientYakuProps.Where(x => x.Targeted).Select(x => $"{x.yaku.Title}: {x.Probability * 100f}%"))}]\n" +
                    $"{State.GetHashCode()} -> New Cards: [{string.Join(";", NewCards)}]\n" +
                    $"{State.GetHashCode()} -> Global Minimum: {GlobalMinimum.yaku.Title} - {GlobalMinimum.Probability * 100}% in {GlobalMinimum.MinTurns} Turns\n" +
                    $"{State.GetHashCode()} -> Local Minimum: {TotalCardValue}\n");

            }
            /* if (Turn)
                 Debug.Log($"Collected Cards: {string.Join(",", NewCards)}\n" +
                     $"Selection from Hand: {State.LastMove.HandSelection}, from Deck {State.LastMove.DeckSelection}" +
                     $"Global {GlobalMinSize}; Local {TotalCardRelevance}; Com Result {result};\n" +
                     $"{string.Join("\n", YakuInTurns.Where(x => YakuTargeted[x.Key]).Select(x => $"{Global.allYaku[x.Key].Title} in min. {x.Value} Turns."))}");
                     */
            return new StateProps() { GlobalMinimum = GlobalMinimum, LocalMinimum = TotalCardValue };
        }

        public override Move RequestDeckSelection(Spielfeld board, Move baseMove)
        {
            throw new NotImplementedException();
        }
    }
}