using System;
using System.Collections.Generic;
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
/// <summary>
/// veraltet
/// </summary>
namespace Hanafuda
{
    public class Yaku : ScriptableObject, IComparable<Yaku>
    {
        public int addPoints;
        public int basePoints;
        public string JName;
        public string Name;
        /// <summary>
        ///     [0] = Benutze Kartentyp,
        ///     [1] = Benutze Kartennamen,
        ///     [0]+[1] = Benutze beides
        /// </summary>
        public int[] Mask = new int[2];

        public int minSize;
        public List<string> Namen = new List<string>();
        public Card.Typen TypPref;
        
        public int CompareTo(Yaku other)
        {
            return Global.allYaku.IndexOf(this).CompareTo(Global.allYaku.IndexOf(other));
        }

        public static void DistinctYakus(List<KeyValuePair<Yaku, int>> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Key.Name.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Key.Name.Contains("kou") &&
                        !x.Key.Name.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Key.Name.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Key.Name.Contains("kou") && !x.Key.Name.Contains("Ameshikou"));
            if (list.Exists(x => x.Key.Name == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Key.Name.Contains("tan") && x.Key.Name != "Aka Ao Kasane");
        }

        public static void DistinctYakus(List<Yaku> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Name.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Name.Contains("kou") && !x.Name.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Name.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Name.Contains("kou") && !x.Name.Contains("Ameshikou"));
            if (list.Exists(x => x.Name == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Name.Contains("tan") && x.Name != "Aka Ao Kasane");
        }

        public static List<Yaku> GetYaku(List<Card> Hand)
        {
            var temp = new List<Yaku>();
            for (var i = 0; i < Global.allYaku.Count; i++)
                if (Hand == Global.allYaku[i])
                    temp.Add(Global.allYaku[i]);
            return temp;
        }

        public override bool Equals(object Right)
        {
            try
            {
                if (this == (List<Card>) Right)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool operator ==(Yaku Left, List<Card> Right)
        {
            if (Right.Count >= Left.minSize)
            {
                if (Left.Mask[0] == 1)
                {
                    var temp = 0;
                    var contains = false;
                    for (var i = 0; i < Right.Count; i++)
                    {
                        if (Right[i].Typ == Left.TypPref)
                            temp++;
                        if (Left.Namen.Contains(Right[i].Name) && Left.Mask[1] != 0)
                            contains = true;
                    }

                    if (temp >= Left.minSize && (contains && Left.Mask[1] == 1 || !contains && Left.Mask[1] == -1 ||
                                                 Left.Mask[1] == 0))
                        return true;
                    return false;
                }

                if (Left.Mask[0] == 0 && Left.Mask[1] == 1)
                {
                    var names = new List<string>(Left.Namen);
                    for (var i = 0; i < Right.Count; i++)
                        if (names.Contains(Right[i].Name))
                            names.Remove(Right[i].Name);
                    if (names.Count == 0)
                        return true;
                    return false;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        public static bool operator !=(Yaku Left, List<Card> Right)
        {
            if (Left == Right)
                return false;
            return true;
        }

        public static bool operator ==(List<Card> Left, Yaku Right)
        {
            if (Right == Left)
                return true;
            return false;
        }

        public static bool operator !=(List<Card> Left, Yaku Right)
        {
            if (Right != Left)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Contains(Card card)
        {
            if (Namen.Contains(card.Name) && Mask[1] == 1 || TypPref == card.Typ && Mask[0] == 1)
                return true;
            return false;
        }
    }
}