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
            { _GlobalWeight, 20 },
            { _LocalWeight, 100 },
            { _CollectionWeight, 5 },

            { _PassWeight,1},
            {_WaitTurnCompensation, 0 },
            {_WaitWeight, 0 },
            {_OpponentInterestWeight, 20 },
            { _PassCollectionWeight, 0 }
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID)
        {
            OmniscientBoard root = new OmniscientBoard(cRoot, playerID);
            root.Turn = true;
            Tree = new OmniscientStateTree(new OmniscientBoard(cRoot, playerID));
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
            Global.Log($"Zustand {State.GetHashCode()}: {State.LastMove.ToString().Replace("\n", "")}");
            if (State.isFinal)
                return Mathf.Infinity;
            if (State.computer.Hand.Count <= 1)
                return 0;

            float Result = 0f;

            StateProps ComStateProps = RateSingleState(State, true);

            OmniscientStateTree PlayerTree = new OmniscientStateTree(State);
            PlayerTree.Build(1, false, true);
            List<OmniscientBoard> PStates = PlayerTree.GetLevel(1);
            List<StateProps> PStateProps = new List<StateProps>();
            List<Card> NewCards = State.computer.CollectedCards.Except(State.parent.computer.CollectedCards).ToList();

            foreach (OmniscientBoard PState in PStates)
                PStateProps.Add(RateSingleState(PState, false));
            float PGlobalMinimum = PStateProps.Max(x => (8 - x.GlobalMinimum.MinTurns) * x.GlobalMinimum.Probability);
            float ComGlobalMinimum = (8 - ComStateProps.GlobalMinimum.MinTurns) * ComStateProps.GlobalMinimum.Probability;
            float GlobalValue = ComGlobalMinimum - PGlobalMinimum;
            float PLocalMinimum = PStateProps.Max(x => x.LocalMinimum);
            float LocalValue = ComStateProps.LocalMinimum - PLocalMinimum;

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
            List<Card> deckCards = new List<Card>();
            for (int turnID = 0; turnID < State.computer.Hand.Count; turnID++)
            {
                deckCards.Add(State.Deck[turnID * 2 + 1]);
            }

            foreach (Card handCard in State.computer.Hand)
            {
                List<Card> fieldMatches = State.Field.FindAll(x => x.Monat == handCard.Monat);
                if (fieldMatches.Count == 0)
                    continue;
                float maxFieldValue = fieldMatches
                    .Max(card => CardProps
                    .First(prop => prop.card.Title == card.Title)
                    .RelevanceForYaku.Sum(rel => rel.Value));

                List<Card> deckMatches = deckCards.FindAll(x => x.Monat == handCard.Monat);
                if (deckMatches.Count == 0)
                    return 0;
                float maxDeckValue = deckMatches
                    .Max(card => CardProps
                    .First(prop => prop.card.Title == card.Title)
                    .RelevanceForYaku.Sum(rel => rel.Value)
                    - ((8 - State.computer.Hand.Count) - deckCards.IndexOf(card)) * weights[_WaitTurnCompensation]);
                if (maxFieldValue > maxDeckValue)
                    return 0;
            }

            return 1;
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
                float value = (8 - yakuProp.MinTurns) * yakuProp.Probability;
                if (GlobalMinimum == null || value < (8 - GlobalMinimum.MinTurns) * GlobalMinimum.Probability)
                    GlobalMinimum = yakuProp;
            }
            if (GlobalMinimum == null)
                GlobalMinimum = OmniscientYakuProps.OrderByDescending(x => x.MinTurns).ToArray()[0];

            try
            {
                TotalCardValue = OmniscientYakuProps
                    .Where(x => x.Targeted && x.Probability > 0)
                    .Sum(x => (8 - x.MinTurns) * x.Probability);
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

        public override Move RequestDeckSelection(Spielfeld board, Move baseMove, int playerID)
        {
            throw new NotImplementedException();
        }
    }
}