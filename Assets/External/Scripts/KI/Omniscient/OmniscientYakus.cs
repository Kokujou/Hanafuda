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

namespace Hanafuda
{
    public partial class OmniscientAI
    {
        public class OmniscientYakus : YakuCollection
        {
            List<CardProperties> CardProps;
            VirtualBoard State;
            bool Turn;
            public OmniscientYakus(List<CardProperties> list, List<Card> newCards, VirtualBoard state, bool turn) : base(list, newCards, state, turn)
            {
                CardProps = list;
                State = state;
                Turn = turn;
                for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                    this.Add(new YakuProperties(yakuID));
                Preparations = () => { };
            }

            protected override CardCollection cardProperties => new OmniscientCards(CardProps, State, Turn);

            protected override void CalcState(VirtualBoard State, bool Turn)
            {
                foreach (Card card in player.CollectedCards)
                    foreach (YakuProperties Prop in this)
                        if (Prop.yaku.Contains(card))
                            Prop.Collected++;
                List<Card> collectables = cardProperties.Where(x => x.Probability > 0).Select(x => x.card).ToList();
                foreach (Card card in collectables)
                    foreach (YakuProperties Prop in this)
                        Prop.IsPossible = true;
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

            protected override void CalcMinTurns(CardCollection cardProperties)
            {
                // Optimierung: Gleichzeitiges Einsammeln von Karten berücksichtigen
                foreach (YakuProperties yakuProp in this)
                {
                    if (!yakuProp.IsPossible) continue;
                    int min = 8, max = 0, count = 0;
                    foreach (CardProperties cardProp in cardProperties)
                    {
                        if (cardProp.MinTurns > 0)
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
                foreach (YakuProperties yakuProp in this)
                {
                    if (!yakuProp.IsPossible) continue;
                    List<float> cardProbs = CardProps.Where(x => yakuProp.yaku.Contains(x.card)).Select(x => x.Probability).ToList();
                    List<int[]> baseTermIDs = BuildCombinations(yakuProp.yaku.minSize, cardProbs.Count - 1);
                    List<float> baseTerms = baseTermIDs.Select(x =>
                    {
                        float result = 1;
                        foreach (int ID in x) result *= cardProbs[ID];
                        return result;
                    }).ToList();
                    IEnumerable<int> numbers = Enumerable.Range(0, cardProbs.Count - 1);
                    List<int[]> addTermIDs = baseTermIDs.Select(x => numbers.Except(x).ToArray()).ToList();
                    List<float> addTerms = addTermIDs.Select(x => x.Sum(y => cardProbs[y])).ToList();
                    yakuProp.Probability = 0;
                    for (int termID = 0; termID < baseTerms.Count; termID++)
                        yakuProp.Probability += baseTerms[termID] * addTerms[termID];
                }
            }
            private List<int[]> BuildCombinations(int outputLength, int numberRange)
            {
                List<int[]> baseTermIDs = new List<int[]>() { Enumerable.Range(0, outputLength).ToArray() };
                int[] last = baseTermIDs[0].ToArray();
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
                            for (newID = factorID; newID < outputLength; newID++)
                                if (newID >= factorID)
                                    last[newID] = last[factorID] + (newID - factorID);

                            baseTermIDs.Add(last.ToArray());
                            if (newID != factorID) factorID = last.Length - 1;
                        }
                    }
                }
                return baseTermIDs;
            }
        }
    }
}