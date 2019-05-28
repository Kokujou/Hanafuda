using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hanafuda
{
    public static class VirtualMethods
    {
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

        public static bool IsOmniscient(this Settings.AIMode mode)
        {
            if (mode == Settings.AIMode.Omniscient || mode == Settings.AIMode.Searching) return true;
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

        public static void Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) val = min;
            else if (val.CompareTo(max) > 0) val = max;
        }

        /// <summary>
        /// Korrigiert den Kamera-Aspekt zu Portrait(mobil) oder Landscape (PC)
        /// </summary>
        public static void SetCameraRect(this Camera cam)
        {
            if (Screen.width >= Screen.height)
                cam.aspect = 16f / 9f;
            else
                cam.aspect = .6f;
        }
        /// <summary>
        /// Generiert eine 1x1 Textur einer Farbe
        /// </summary>
        /// <returns></returns>
        public static Texture2D CreateTexture(this Color color)
        {
            var result = new Texture2D(1, 1);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }

        public static T GetRandom<T>(this List<T> list, Func<T, bool> exclude = null)
        {
            int index = Random.Range(0, list.Count);
            if (list.Count == 1) return list[0];
            while (exclude != null && exclude(list[index]))
                index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}