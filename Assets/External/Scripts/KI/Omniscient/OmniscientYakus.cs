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
                    int count = 0;
                    float baseProb = 1;
                    float addProb = 0;
                    foreach (CardProperties cardProp in cardProperties)
                    {
                        if (!yakuProp.yaku.Contains(cardProp.card)) continue;
                        if (cardProp.Probability > 0)
                        {
                            count++;
                            baseProb *= cardProp.Probability;
                            if (count >= yakuProp.yaku.minSize - 1)
                            {
                                addProb += cardProp.Probability;
                            }
                        }
                    }
                    yakuProp.Probability = baseProb * addProb;
                }
            }
        }
    }
}