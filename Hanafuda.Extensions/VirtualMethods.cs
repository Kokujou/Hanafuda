using Hanafuda.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hanafuda.Extensions
{
    public static class VirtualMethods
    {
        /// <summary>
        /// Determines if the field consists out of 4 matching cards and needs a redeal
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool NeedsRedeal(this List<Card> field)
            => field.Exists(x => field.Count(y => y.Monat == x.Monat) == 4);

        /// <summary>
        /// Determines, if a hand is an initial win
        /// </summary>
        /// <param name="hand">input hand with 8 cards</param>
        /// <returns>0: no win, 1: 4 matching cards, 2: 4 pairs</returns>
        public static int IsInitialWin(this List<Card> hand)
        {
            if (hand.Count != 8) return 0;
            Dictionary<Card.Months, int> months = Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => 0);
            foreach (Card card in hand)
                months[card.Monat]++;
            int pairs = 0;
            foreach (var pair in months)
                if (pair.Value == 4) return 1;
                else if (pair.Value == 2) pairs++;
            if (pairs == 4) return 2;
            return 0;
        }

        public static bool IsOmniscient(this AIMode mode)
        {
            if (mode == AIMode.Omniscient || mode == AIMode.Searching) return true;
            return false;
        }

        public static Dictionary<int, T> ToDictionary<T>(this List<T> list, int Capacity = -1)
        {
            Dictionary<int, T> Result;
            if (Capacity < 0)
                Result = new Dictionary<int, T>();
            else
                Result = new Dictionary<int, T>(Capacity);
            for (int id = 0; id < list.Count; id++)
                Result.Add(id, list[id]);
            return Result;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            T toCompare = val;
            if (toCompare.CompareTo(min) < 0) toCompare = min;
            else if (toCompare.CompareTo(max) > 0) toCompare = max;
            return toCompare;
        }
        
        public static T GetRandom<T>(this List<T> list, Func<T, bool> exclude = null)
        {
            Random rnd = new Random();
            int index = rnd.Next(0, list.Count);
            if (list.Count == 1) return list[0];
            while (exclude != null && exclude(list[index]))
                index = rnd.Next(0, list.Count);
            return list[index];
        }
    }
}