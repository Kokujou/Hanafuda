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
        const float _MinSizeWeight = 1;

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
             */
            float result = 0;

            int ComNextYaku = 0;
            int PNextYaku = 0;

            float TotalCardRelevance = 0;

            Player Com = State.players[1 - Settings.PlayerID];
            Player P1 = State.players[Settings.PlayerID];

            SortedList<int, float> ComYakuValues = new SortedList<int, float>();
            SortedList<int, float> P1YakuValues = new SortedList<int, float>();

            SortedList<Card, float> ComCardProb = new SortedList<Card, float>();

            List<Card> ComNewCards = Com.CollectedCards.Where(
                x => !Tree.GetState(State.parentCoords.x, State.parentCoords.y)
                .players[1 - Settings.PlayerID].CollectedCards
                .Contains(x)).ToList();

            foreach (Card card in Com.CollectedCards)
            {
                ComCardProb.Add(card, 1);
                for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                {
                    Yaku yaku = Global.allYaku[yakuID];
                    int value = yaku.Contains(card) ? 1 : 0;
                    if (ComYakuValues.ContainsKey(yakuID))
                        ComYakuValues[yakuID] += value;
                    else
                        ComYakuValues.Add(yakuID, value);
                }
            }

            List<Card> ComCollectables = new List<Card>(State.Field);
            for (int i = 0; i < P1.Hand.Count; i++)
            {
                ComCollectables.Add(State.Deck[i * 2 + 1]);
                ComCollectables.Add(P1.Hand[i]);
            }

            List<Card.Months> PlayableMonths = new List<Card.Months>();
            for (int i = 0; i < Com.Hand.Count; i++)
            {
                ComCardProb.Add(State.Deck[i * 2], 1);
                PlayableMonths.Add(State.Deck[i].Monat);
                ComCardProb.Add(Com.Hand[i], 1);
                PlayableMonths.Add(Com.Hand[i].Monat);
            }
            PlayableMonths = PlayableMonths.Distinct().ToList();

            foreach (Card card in ComCollectables)
            {
                if (PlayableMonths.Contains(card.Monat))
                {
                    ComCardProb.Add(card, 1);
                }
            }

            List<int> possibleYakus = Yaku.GetYakuIDs(ComCardProb.Keys.ToList());
            for (int i = 0, j = 0; i < ComYakuValues.Count; i++)
            {
                if (possibleYakus[j] != i)
                    ComYakuValues.RemoveAt(i);
                else
                    j++;
            }

            ComNextYaku = Global.allCards.Count;
            List<string> targetedYakus = new List<string>();
            foreach (KeyValuePair<int, float> yaku in ComYakuValues)
            {
                bool isTargeted = false;
                foreach (Card card in ComNewCards)
                {
                    if (Global.allYaku[yaku.Key].Contains(card))
                    {
                        isTargeted = true;
                        targetedYakus.Add(Global.allYaku[yaku.Key].Title);
                        break;
                    }
                }
                if (!isTargeted) continue;
                int yakuIn = Global.allYaku[yaku.Key].minSize - (int)yaku.Value;
                if (yakuIn < 1)
                {
                    if (Global.allYaku[yaku.Key].addPoints > 0)
                        yakuIn = 1;
                    else
                        continue;
                }
                if (yakuIn < ComNextYaku)
                {
                    ComNextYaku = yakuIn;
                }
            }
            Debug.Log(string.Join(",",targetedYakus));

            foreach (KeyValuePair<Card, float> card in ComCardProb)
            {
                TotalCardRelevance += card.Value * CardValues[Global.allCards.IndexOf(card.Key)].GetValue(ComYakuValues);
            }

            result = (PNextYaku - ComNextYaku) * _MinSizeWeight
                + TotalCardRelevance * _TCRWeight;
            Debug.Log($"{State.LastMove.HandSelection}: {result}");
            return result;
        }

    }
}