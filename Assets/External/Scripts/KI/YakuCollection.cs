using System;
using System.Collections.Generic;
using System.Linq;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

namespace Hanafuda
{
    public class YakuCollection : List<YakuProperties>
    {
        protected List<CardProperties> CardProps;
        protected void CalcTargets(List<Card> newCards)
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
        protected void CalcState(List<Card> activeCollection, int roundsLeft)
        {
            foreach (Card card in activeCollection)
                foreach (YakuProperties Prop in this)
                    if (Prop.yaku.Contains(card))
                        Prop.Collected++;
            var collectables = CardProps.Where(x => x.Probability > 0);
            foreach (YakuProperties yakuProp in this)
            {
                yakuProp.IsPossible = false;
                if (yakuProp.MinTurns > roundsLeft) continue;
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

        protected void CalcMinTurns()
        {
            List<CardProperties> cardProperties = CardProps.ToList();
            // Optimierung: Gleichzeitiges Einsammeln von Karten berücksichtigen
            foreach (YakuProperties yakuProp in this)
            {
                int min = 8, max = 0, count = 0;
                List<int> cardDurations = cardProperties
                    .Where(x => yakuProp.yaku.Contains(x.card) && x.Probability > 0)
                    .Select(x => x.MinTurns).ToList();
                cardDurations.Sort();
                for (int sizeID = 0; sizeID < cardDurations.Count && count < yakuProp.yaku.minSize; sizeID++)
                {
                    int size = cardDurations[sizeID];
                    if (size >= 0)
                    {
                        count++;
                        if (size > max) max = size;
                        if (size < min) min = size;
                    }
                }
                yakuProp.MinTurns = (min - 1) + count;
                if (yakuProp.MinTurns < max) yakuProp.MinTurns = max;
            }
        }

        protected void CalcProbs()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            foreach (YakuProperties yakuProp in this.Where(x => x.IsPossible))
            {
                List<double> cardProbs = CardProps.Where(x => yakuProp.yaku.Contains(x.card)).Select(x => (double)x.Probability).ToList();
                yakuProp.Probability = CalcYakuProb(yakuProp.yaku.minSize, cardProbs);
            }
            //Debug.Log(string.Join("\n", this.Select(x => $"{x.yaku.Title}: {x.Probability}")));
        }
        private float CalcYakuProb(int minSize, List<double> cardProbs)
        {
            double result = 1;
            cardProbs.Sort();

            int probID;
            for (probID = cardProbs.Count - 1; probID >= cardProbs.Count - minSize; probID--)
                result *= cardProbs[probID];

            double sum = 0;
            while (probID >= 0)
            {
                sum += cardProbs[probID];
                probID--;
            }
            if (minSize < cardProbs.Count)
                result = result + ((1 - result) * (sum / (cardProbs.Count - minSize)));

            return (float)result;
        }

        public YakuCollection(List<CardProperties> cardProperties, List<Card> newCards, List<Card> activeCollection, int roundsLeft) : base()
        {
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                this.Add(new YakuProperties(yakuID));

            CardProps = cardProperties;

            CalcTargets(newCards);
            CalcMinTurns();
            CalcState(activeCollection, roundsLeft);
            CalcProbs();
        }
    }
}