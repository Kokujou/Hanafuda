using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class OmniscientAI : KI
    {
        const string _LocalWeight = "_LocalWeight";
        const string _GlobalWeight = "_GlobalWeight";

        public int _MaxPTurns = 3;

        List<CardProperties> CardProps = new List<CardProperties>();

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _GlobalWeight, 1f },
            { _LocalWeight, 1f }
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public override void BuildStateTree(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            Tree = new StateTree(cRoot);
            Tree.Build(1);
        }

        public OmniscientAI(string name) : base(name)
        {
            for (int i = 0; i < Global.allCards.Count; i++)
            {
                CardProps.Add(new CardProperties(i));
            }
        }

        public override float RateState(VirtualBoard State)
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
            lock (StateTree.thisLock)
                Global.Log($"Zustand {State.GetHashCode()}: {PlayerAction.FromMove(State.LastMove, MainSceneVariables.boardTransforms.Main).ToString().Replace("\n", "")}");
            if (State.isFinal) return Mathf.Infinity;

            float Result = 0f;

            Player Com = State.active;
            Player P1 = State.opponent;

            StateProps ComStateProps = RateSingleState(State, true);

            StateTree PlayerTree = new StateTree(State);
            PlayerTree.Build(1, false, true);
            List<VirtualBoard> PStates = PlayerTree.GetLevel(1);
            List<StateProps> PStateProps = new List<StateProps>();

            foreach (VirtualBoard PState in PStates)
                PStateProps.Add(RateSingleState(PState, false));

            float PLocalMinimum = 0;
            float PGlobalMinimum = 0;
            if (PStateProps.Count > 0)
            {
                PLocalMinimum = PStateProps.Max(x => x.LocalMinimum);
                PGlobalMinimum = PStateProps.Max(x => x.GlobalMinimum);
            }

            Result = (ComStateProps.GlobalMinimum - PGlobalMinimum) * weights[_GlobalWeight]
                + (ComStateProps.LocalMinimum - PLocalMinimum) * weights[_LocalWeight];

            return Result;
        }

        public struct StateProps
        {
            public float GlobalMinimum;
            public float LocalMinimum;
        }

        public StateProps RateSingleState(VirtualBoard State, bool Turn)
        {
            Player Com = State.active;
            Player P1 = State.opponent;

            float GlobalMinimum = 0;

            float TotalCardValue = 0;

            List<Card> NewCards = new List<Card>();
            if (State.LastMove != null)
            {
                NewCards = (Turn ? State.active : State.opponent).CollectedCards
                .Where(x =>
                {
                    VirtualBoard state = Tree.GetState(State.parentCoords.x, State.parentCoords.y);
                    return !(Turn ? state.active : state.opponent).CollectedCards.Contains(x);
                }).ToList();
            }

            OmniscientYakus OmniscientYakuProps = new OmniscientYakus(CardProps, NewCards, State, Turn);

            foreach (YakuProperties yakuProp in OmniscientYakuProps)
            {
                float value = (P1.Hand.Count - yakuProp.MinTurns) * yakuProp.Probability;
                if (value < GlobalMinimum)
                    GlobalMinimum = value;
            }

            try
            {
                TotalCardValue = OmniscientYakuProps
                    .Where(x => x.Targeted)
                    .Max(x => (P1.Hand.Count - x.MinTurns) * x.Probability);
            }
            catch (Exception) { }

            if (Turn)
            {
                Global.Log($"{State.GetHashCode()} -> YakuProps: [{string.Join(";", OmniscientYakuProps.Where(x => x.Probability > 0).Select(x => $"{x.yaku.Title}: {x.Probability * 100f}%"))}]\n" +
                    $"{State.GetHashCode()} -> New Cards: [{string.Join(";", NewCards)}]\n" +
                    $"{State.GetHashCode()} -> Global Minimum: {GlobalMinimum}\n" +
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
    }
}