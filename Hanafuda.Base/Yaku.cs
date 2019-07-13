using System;
using System.Collections.Generic;
using Hanafuda.Base.Interfaces;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

namespace Hanafuda.Base
{
    [Serializable]
    public class Yaku
    {
        public int addPoints;
        public int basePoints;
        public string JName;
        public string Title;

        /// <summary>
        ///     [0] = Benutze Kartentyp,
        ///     [1] = Benutze Kartennamen,
        ///     [0]+[1] = Benutze beides
        /// </summary>
        public int[] Mask = new int[2];

        public int minSize;
        public List<string> Namen = new List<string>();
        public CardMotive TypePref;

        public int GetPoints(int collected)
        {
            if (collected < minSize) return 0;
            else if (collected == minSize) return basePoints;
            else if (addPoints > 0) return (basePoints + (addPoints * (collected - minSize)));
            else return 0;
        }

        public override bool Equals(object Right) => Right is List<ICard> && this == (List<ICard>)Right;

        public static bool operator ==(Yaku yaku, List<ICard> Cards)
        {
            if (Cards.Count >= yaku.minSize)
            {
                int matches = 0;
                int nameMatches = 0;
                foreach (Card card in Cards)
                {
                    if (yaku.Contains(card))
                        matches++;
                    if (yaku.Namen.Contains(card.Title))
                        nameMatches++;
                }
                if (matches >= yaku.minSize && ((yaku.Mask[0] == 1 && yaku.Mask[1] == 1) || nameMatches >= yaku.Namen.Count))
                    return true;
                else return false;
            }
            else
                return false;
        }

        public static bool operator !=(Yaku Left, List<ICard> Right) => Left == Right;

        public static bool operator ==(List<ICard> Left, Yaku Right) => Right == Left;

        public static bool operator !=(List<ICard> Left, Yaku Right) => Right != Left;

        public override int GetHashCode() => base.GetHashCode();

        public bool Contains(ICard card)
        {
            if (Mask[1] == -1 && Namen.Contains(card.Title))
                return false;
            if ((Mask[0] == 1 && card.Motive == TypePref) ||
                (Mask[1] == 1 && Namen.Contains(card.Title)))
                return true;
            return false;
        }
        public override string ToString() => Title;
    }
}