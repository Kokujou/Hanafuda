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

        const float _TotalValueWeight = 1f;
        const float _GlobalWeight = 100f;
        const float _PValueWeight = 0.1f;
        const float _ComValueWeight = 1f;

        const int _MaxPTurns = 3;

        List<CardProperties> CardProps = new List<CardProperties>();

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

            if (State.isFinal) return Mathf.Infinity;

            float Result = 0f;

            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];

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

            return Result;
        }

        public float RateSingleState(VirtualBoard State, bool Turn)
        {
            float result = 0;
            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];

            float GlobalMinimum = 0;

            float TotalCardValue = 0;

            List<Card> NewCards = new List<Card>();
            if (State.LastMove != null)
            {
                NewCards = State.players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID].CollectedCards
                .Where(x => !Tree.GetState(State.parentCoords.x, State.parentCoords.y)
                .players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID].CollectedCards
                .Contains(x)).ToList();
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