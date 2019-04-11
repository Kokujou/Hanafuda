/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class OmniscientAI
    {
        public class OmniscientYakus : YakuCollection
        {
            protected override void Preparations() => cardProperties = new OmniscientCards(CardProps, State, Turn);

            public OmniscientYakus(List<CardProperties> list, List<Card> newCards, VirtualBoard state, bool turn) : base(list, newCards, state, turn) { }

            protected override void CalcState(VirtualBoard State, bool Turn)
            {
                int maxTurns = State.players[1 - Settings.PlayerID].Hand.Count;
                foreach (Card card in player.CollectedCards)
                    foreach (YakuProperties Prop in this)
                        if (Prop.yaku.Contains(card))
                            Prop.Collected++;
                var collectables = cardProperties.Where(x => x.Probability > 0);
                foreach (YakuProperties yakuProp in this)
                {
                    yakuProp.IsPossible = false;
                    if (yakuProp.MinTurns > maxTurns) continue;
                    byte matching = 0;
                    foreach (CardProperties cardProp in collectables)
                    {
                        if (yakuProp.yaku.Contains(cardProp.card))
                            matching++;
                    }
                    if (matching >= yakuProp.yaku.minSize)
                        yakuProp.IsPossible = true;

                }
            }

            protected override void CalcTargets(List<Card> newCards)
            {
                if (State.LastMove != null)
                {
                    for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                    {
                        foreach (Card newCard in newCards)
                        {
                            if (Global.allYaku[yakuID].Contains(newCard))
                            {
                                this[yakuID].Targeted = true;
                                break;
                            }
                        }
                    }
                }
            }

            protected override void CalcMinTurns(CardCollection Properties)
            {
                List<CardProperties> cardProperties = Properties.ToList();
                cardProperties.Sort();
                // Optimierung: Gleichzeitiges Einsammeln von Karten berücksichtigen
                foreach (YakuProperties yakuProp in this)
                {
                    int min = 8, max = 0, count = 0;
                    for (int propID = 0; propID < cardProperties.Count || propID < yakuProp.yaku.minSize; propID++)
                    {
                        CardProperties cardProp = cardProperties[propID];
                        if (cardProp.MinTurns > 0 && yakuProp.yaku.Contains(cardProp.card) && cardProp.Probability > 0)
                        {
                            count++;
                            if (cardProp.MinTurns > max) max = cardProp.MinTurns;
                            if (cardProp.MinTurns < min) min = cardProp.MinTurns;
                        }
                    }
                    yakuProp.MinTurns = (min - 1) + count;
                    if (yakuProp.MinTurns < max) yakuProp.MinTurns = max;
                }
            }

            protected override void CalcProbs(CardCollection cardProperties)
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                foreach (YakuProperties yakuProp in this.Where(x => x.IsPossible))
                {
                    double[] cardProbs = CardProps.Where(x => yakuProp.yaku.Contains(x.card)).Select(x => (double)x.Probability).ToArray();
                    yakuProp.Probability = CalcYakuProb(yakuProp.yaku.minSize, cardProbs);
                }
                //Debug.Log(string.Join("\n", this.Select(x => $"{x.yaku.Title}: {x.Probability}")));
            }
            private float CalcYakuProb(int minSize, double[] cardProbs)
            {
                double result = 1;
                List<double> SortedProbs = cardProbs.ToList();
                SortedProbs.Sort();
                double[] Max = SortedProbs.Skip(cardProbs.Length - minSize).ToArray();
                foreach (double prob in Max) result *= prob;
                result = System.Math.Pow(result, 1 / cardProbs.Length);
                return (float)result;
            }
            /*
            private float CalcYakuProb(int outputLength, int numberRange, double[] cardProbs)
            {
                int[] last = Enumerable.Range(0, outputLength).ToArray();
                double result = 1;
                foreach (int factorID in last) result *= cardProbs[factorID];
                double add = 0;
                for (int sumID = last[last.Length - 1] + 1; sumID <= numberRange; sumID++)
                    add += cardProbs[sumID];
                result *= add;
                for (int factorID = last.Length - 1; factorID >= 0; factorID--)
                {
                    while (true)
                    {
                        if (last[factorID] >= numberRange) break;
                        else if (factorID + 1 < last.Length && last[factorID] + 1 == last[factorID + 1]) break;
                        else
                        {
                            last[factorID]++;
                            int newID;
                            for (newID = factorID + 1; newID < outputLength; newID++)
                                last[newID] = last[factorID] + (newID - factorID);

                            double term = 1;
                            foreach (int facID in last) term *= cardProbs[facID];
                            double addFactor = 0;
                            for (int summand = last[last.Length - 1] + 1; summand <= numberRange; summand++)
                                addFactor += cardProbs[summand];
                            term *= addFactor;
                            result += term;

                            if (newID != factorID) factorID = last.Length - 1;
                        }
                    }
                }

                return (float)result;
            }*/
        }
    }
}