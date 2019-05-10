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
        const string _TotalValueWeightS = "_TotalValueWeight";
        const string _GlobalWeightS = "_GlobalWeight";
        const string _PValueWeightS = "_PValueWeight";
        const string _ComValueWeightS = "_ComValueWeight";

        public float _TotalValueWeight = 1f;
        public float _GlobalWeight = 100f;
        public float _PValueWeight = 0.1f;
        public float _ComValueWeight = 1f;

        public int _MaxPTurns = 3;

        List<CardProperties> CardProps = new List<CardProperties>();

        public override Dictionary<string, float> GetWeights()
        {
            return new Dictionary<string, float>()
            {
                { _TotalValueWeightS, _TotalValueWeight },
                { _GlobalWeightS, _GlobalWeight},
                { _PValueWeightS, _PValueWeight},
                { _ComValueWeightS, _ComValueWeight}
            };
        }

        public override void SetWeight(string name, float value)
        {
            switch (name)
            {
                case _TotalValueWeightS:
                    _TotalValueWeight = value;
                    break;
                case _PValueWeightS:
                    _PValueWeight = value;
                    break;
                case _ComValueWeightS:
                    _ComValueWeight = value;
                    break;
                case _GlobalWeightS:
                    _GlobalWeight = value;
                    break;
                default: break;
            }
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

            float ComValue = RateSingleState(State, true);

            StateTree PlayerTree = new StateTree(State);
            PlayerTree.Build(1, false, true);
            List<VirtualBoard> PStates = PlayerTree.GetLevel(1);
            List<float> PStateValues = new List<float>();

            foreach (VirtualBoard PState in PStates)
                PStateValues.Add(RateSingleState(PState, false));
            PStateValues.Sort();

            PStateValues = PStateValues.Skip(PStateValues.Count - 3).ToList();

            float PValue = 0;
            if (PStateValues.Count > 0)
                PValue = PStateValues.Average(x => x);

            Result = ComValue * _ComValueWeight
                - PValue * _PValueWeight;
            lock (StateTree.thisLock)
            {
                Global.Log($"{State.GetHashCode()} -> Com Value: {ComValue}");
                Global.Log($"{State.GetHashCode()} -> Player Value: {PValue}");
            }

            return Result;
        }

        public float RateSingleState(VirtualBoard State, bool Turn)
        {
            float result = 0;
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

            TotalCardValue = OmniscientYakuProps
                .Where(x => x.Targeted)
                .Sum(x => (P1.Hand.Count - x.MinTurns) * x.Probability);

            result = GlobalMinimum * _GlobalWeight
                + TotalCardValue * _TotalValueWeight;
            if (Turn)
                lock (StateTree.thisLock)
                {
                    Global.Log($"{State.GetHashCode()} -> YakuProps: {string.Join(";", OmniscientYakuProps.Select(x => x.Probability))}");
                    Global.Log($"{State.GetHashCode()} -> New Cards: {string.Join(";", NewCards)}");
                    Global.Log($"{State.GetHashCode()} -> Global Minimum: {GlobalMinimum}");
                    Global.Log($"{State.GetHashCode()} -> Total Card Value: {TotalCardValue}");
                }
            /* if (Turn)
                 Debug.Log($"Collected Cards: {string.Join(",", NewCards)}\n" +
                     $"Selection from Hand: {State.LastMove.HandSelection}, from Deck {State.LastMove.DeckSelection}" +
                     $"Global {GlobalMinSize}; Local {TotalCardRelevance}; Com Result {result};\n" +
                     $"{string.Join("\n", YakuInTurns.Where(x => YakuTargeted[x.Key]).Select(x => $"{Global.allYaku[x.Key].Title} in min. {x.Value} Turns."))}");
                     */
            return result;
        }
    }
}