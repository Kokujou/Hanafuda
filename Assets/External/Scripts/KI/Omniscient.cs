using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class OmniscientAI : KI
    {
        const float _TCRWeight = 1;
        const float _MinSizeWeight = 100;

        List<Yaku> ComPossibleYaku = new List<Yaku>();
        List<Yaku> PPossibleYaku = new List<Yaku>();

        List<CardValue> CardValues = new List<CardValue>();

        private class CardValue
        {
            public SortedList<int, float> RelevanceForYaku = new SortedList<int, float>();
            public CardValue(int cardID)
            {
                Card card = Global.allCards[cardID];
                for (int i = 0; i < Global.allYaku.Count; i++)
                {
                    Yaku yaku = Global.allYaku[i];
                    if (yaku.Contains(card))
                    {
                        RelevanceForYaku.Add(i, 1 / yaku.minSize);
                    }
                }
            }
            public float GetValue(SortedList<int, float> YakuValues)
            {
                float result = 0;
                for (int i = 0; i < YakuValues.Count; i++)
                {
                    float value = 0;
                    RelevanceForYaku.TryGetValue(i, out value);
                    value *= YakuValues.Values[i];
                    result += value;
                }
                return result;
            }
        }

        public OmniscientAI(string name) : base(name)
        {
            for (int i = 0; i < Global.allCards.Count; i++)
                CardValues.Add(new CardValue(i));
        }

        public override Move MakeTurn(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            Tree = new StateTree(cRoot);
            Tree.Build(1);
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            for (var i = 0; i < Tree.GetLevel(1).Count; i++)
            {
                Tree.GetState(1, i).Value = RateState(Tree.GetState(1, i));
                if (Tree.GetState(1, i).Value > maxValue)
                {
                    maxValue = Tree.GetState(1, i).Value;
                    selectedMove = Tree.GetState(1, i).LastMove;
                }
            }
            return selectedMove;
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
             */
            if (State.isFinal) return Mathf.Infinity;
            float result = 0;

            uint ComGlobalMinSize = 0;
            uint PGlobalMinSize = 0;

            float TotalCardRelevance = 0;

            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];

            SortedList<int, float> ComYakuProbs;
            SortedList<int, uint> YakuInTurns;
            SortedList<int, bool> YakuTargeted;
            CalcYakuProps(out ComYakuProbs, out YakuInTurns, out YakuTargeted, State);

            ComGlobalMinSize = YakuInTurns.Min(x => x.Value);

            TotalCardRelevance = YakuInTurns
                .Where(x => YakuTargeted[x.Key])
                .Sum(x => Mathf.Pow(10, 1f / x.Value));

            result = ((float)PGlobalMinSize - ComGlobalMinSize) * _MinSizeWeight
                + TotalCardRelevance * _TCRWeight;
            Debug.Log($"{State.LastMove.HandSelection}: Global {ComGlobalMinSize}; Local {TotalCardRelevance}; Result {result}");
            Debug.Log(string.Join("\n", YakuInTurns.Where(x => YakuTargeted[x.Key]).Select(x=>Global.allYaku[x.Key].Title)));
            return result;
        }

        private void CalcYakuProps(out SortedList<int, float> YakuProbs, out SortedList<int, uint> YakuIn, out SortedList<int, bool> YakuTargeted, VirtualBoard State)
        {
            YakuProbs = new SortedList<int, float>();
            YakuIn = new SortedList<int, uint>();

            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];

            SortedList<Card, float> ComCardProb;
            SortedList<Card, uint> ComCardIn;
            CalcCardProps(out ComCardProb, out ComCardIn, State);

            List<int> possibleYakus = Yaku.GetYakuIDs(ComCardProb.Keys.ToList());

            List<Card> ComNewCards = Com.CollectedCards.Where(
                x => !Tree.GetState(State.parentCoords.x, State.parentCoords.y)
                .players[1 - Settings.PlayerID].CollectedCards
                .Contains(x)).ToList();
            YakuTargeted = new SortedList<int, bool>();
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
            {
                YakuTargeted.Add(yakuID, false);
                foreach (Card newCard in ComNewCards)
                {
                    if (Global.allYaku[yakuID].Contains(newCard))
                    {
                        YakuTargeted[yakuID] = true;
                        break;
                    }
                }
            }

            for (int yakuID = 0, pYakuID = 0; yakuID < Global.allYaku.Count && pYakuID < possibleYakus.Count; yakuID++)
            {
                if (possibleYakus[pYakuID] == yakuID)
                {
                    foreach (KeyValuePair<Card, uint> card in ComCardIn)
                    {
                        if (card.Value == 0)
                        {
                            if (!YakuIn.ContainsKey(yakuID))
                                YakuIn.Add(yakuID, 0);
                            else if (card.Value > YakuIn[yakuID])
                                YakuIn[yakuID] = card.Value;

                            Yaku yaku = Global.allYaku[yakuID];
                            int value = yaku.Contains(card.Key) ? 1 : 0;
                            if (YakuProbs.ContainsKey(yakuID))
                                YakuProbs[yakuID] += value;
                            else
                                YakuProbs.Add(yakuID, value);
                        }
                    }
                    uint InCards = (uint)Global.allYaku[yakuID].minSize - (uint)YakuProbs[yakuID];
                    if (YakuIn[yakuID] < InCards)
                        YakuIn[yakuID] = InCards;
                    pYakuID++;
                }
            }
        }

        private void CalcCardProps(out SortedList<Card, float> CardProbs, out SortedList<Card, uint> CardIn, VirtualBoard State)
        {
            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];
            CardProbs = new SortedList<Card, float>();
            CardIn = new SortedList<Card, uint>();

            foreach (Card card in Com.CollectedCards)
            {
                CardProbs.Add(card, 1);
                CardIn.Add(card, 0);
            }

            List<Card.Months> PlayableMonths = new List<Card.Months>();
            for (int cardID = 0; cardID < Com.Hand.Count; cardID++)
            {
                CardProbs.Add(State.Deck[cardID * 2], 1);
                CardIn.Add(State.Deck[cardID * 2], (uint)cardID + 1);
                PlayableMonths.Add(State.Deck[cardID].Monat);
                CardProbs.Add(Com.Hand[cardID], 1);
                CardIn.Add(Com.Hand[cardID], 1);
                PlayableMonths.Add(Com.Hand[cardID].Monat);
            }
            PlayableMonths = PlayableMonths.Distinct().ToList();

            for (int cardID = 0; cardID < P1.Hand.Count; cardID++)
            {
                Card handCard = State.Deck[cardID * 2 + 1];
                Card deckCard = P1.Hand[cardID];
                if (PlayableMonths.Contains(handCard.Monat))
                {
                    CardProbs.Add(handCard, 1);
                    CardIn.Add(handCard, 1);
                }
                if (PlayableMonths.Contains(deckCard.Monat))
                {
                    CardProbs.Add(deckCard, cardID * 2 + 1);
                    CardIn.Add(deckCard, (uint)cardID * 2 + 1);
                }
            }

            for (int cardID = 0; cardID < State.Field.Count; cardID++)
            {
                if (PlayableMonths.Contains(State.Field[cardID].Monat))
                {
                    CardProbs.Add(State.Field[cardID], 1);
                    CardIn.Add(State.Field[cardID], 1);
                }
            }


        }
    }
}