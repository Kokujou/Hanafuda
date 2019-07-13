
using Hanafuda.Base;
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

namespace Hanafuda
{
    public class Yaku3D : ScriptableObject
    {
        public Yaku VirtualYaku;

        public static implicit operator Yaku3D(Yaku yaku)
        {
            var yaku3D = CreateInstance<Yaku3D>();
            yaku3D.VirtualYaku = yaku;
            return yaku3D;
        }

        public static implicit operator Yaku(Yaku3D yaku3D) => yaku3D.VirtualYaku;
    }
}