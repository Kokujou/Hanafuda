using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
    public class Yaku : ScriptableObject, IComparable<Yaku>
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
        public Card.Type TypePref;

        public int CompareTo(Yaku other)
            => Global.allYaku.IndexOf(this).CompareTo(Global.allYaku.IndexOf(other));

        public int GetPoints(int collected)
        {
            if (collected < minSize)
                return 0;
            else if (collected == minSize)
                return basePoints;
            else if (addPoints > 0)
                return (basePoints + (addPoints * (collected - minSize)));
            else
                return 0;
        }

        public static List<Yaku> GetNewYakus(Player player, List<Card> newCards)
            => GetNewYakus(player.CollectedYaku, newCards);

        public static List<Yaku> GetNewYakus(List<Card> collection)
            => GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), collection);

        public static List<Yaku> GetNewYakus(Dictionary<int, int> currentYakus, List<Card> newCards, bool AllowWrite = false)
        {
            List<Yaku> NewYaku = new List<Yaku>();
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
            {
                Yaku yaku = Global.allYaku[yakuID];
                int matchingCount = 0;
                foreach (Card card in newCards)
                {
                    if (!yaku.Contains(card))
                        continue;
                    int oldCount = currentYakus[yakuID];
                    matchingCount++;
                    if (AllowWrite)
                        currentYakus[yakuID]++;
                    if (yaku.GetPoints(oldCount + matchingCount) > yaku.GetPoints(oldCount))
                        NewYaku.Add(yaku);
                }
            }
            return NewYaku;
        }

        public static void DistinctYakus(List<KeyValuePair<Yaku, int>> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Key.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Key.Title.Contains("kou") &&
                        !x.Key.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Key.Title.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Key.Title.Contains("kou") && !x.Key.Title.Contains("Ameshikou"));
            if (list.Exists(x => x.Key.Title == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Key.Title.Contains("tan") && x.Key.Title != "Aka Ao Kasane");
        }

        public static void DistinctYakus(List<Yaku> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Title.Contains("kou") && !x.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Title.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Title.Contains("kou") && !x.Title.Contains("Ameshikou"));
            if (list.Exists(x => x.Title == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Title.Contains("tan") && x.Title != "Aka Ao Kasane");
        }

        public static List<Yaku> GetYaku(List<Card> Hand)
        {
            var temp = new List<Yaku>();
            for (var i = 0; i < Global.allYaku.Count; i++)
                if (Hand == Global.allYaku[i])
                    temp.Add(Global.allYaku[i]);
            return temp;
        }

        public static List<int> GetYakuIDs(List<Card> Hand)
        {
            var temp = new List<int>();
            for (var i = 0; i < Global.allYaku.Count; i++)
                if (Hand == Global.allYaku[i])
                    temp.Add(i);
            return temp;
        }

        public override bool Equals(object Right)
            => Right is List<Card> collection && this == collection;

        public static bool operator ==(Yaku yaku, List<Card> Cards)
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
                else
                    return false;
            }
            else
                return false;
        }

        public static bool operator !=(Yaku Left, List<Card> Right)
            => !(Left == Right);

        public static bool operator ==(List<Card> Left, Yaku Right)
            => Right == Left;

        public static bool operator !=(List<Card> Left, Yaku Right)
            => Right != Left;

        public override int GetHashCode()
            => Title.GetHashCode();

        public bool Contains(Card card)
        {
            if (Mask[1] == -1 && Namen.Contains(card.Title))
                return false;
            if ((Mask[0] == 1 && card.Typ == TypePref) ||
                (Mask[1] == 1 && Namen.Contains(card.Title)))
                return true;
            return false;
        }
        public override string ToString() => Title;
    }
}