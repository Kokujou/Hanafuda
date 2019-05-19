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
        public static void Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) val =  min;
            else if (val.CompareTo(max) > 0) val =  max;
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