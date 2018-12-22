using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public static class VirtualMethods
    {
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
    }
}