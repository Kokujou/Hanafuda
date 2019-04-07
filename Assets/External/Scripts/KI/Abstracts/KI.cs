﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class KI : Player
    {
        /*
         * Berechnung der Mindestzüge über minTurn + >minturn der Karte
         * Idee für unwissende KI: Gegner legt Karte aufs Feld -> 
         *      - keine Monat der Feldkarten ist nicht in den Handkarten enthalten, und/oder
         *      - Plan ist gespielte Karte einzusammeln
         * 
         */
        public enum Mode
        {
            Statistic,
            Searching,
            Omniscient
        }
        public abstract Move MakeTurn(VirtualBoard board);
        public abstract float RateState(VirtualBoard state);

        public StateTree Tree;

        public KI(string name) : base(name)
        {
            Tree = new StateTree();
        }

        public static KI Init(Mode mode, string name)
        {
            switch (mode)
            {
                case Mode.Omniscient:
                    return new OmniscientAI(name);
                case Mode.Searching:
                    return new SearchingAI(name);
                case Mode.Statistic:
                    return new CalculatingAI(name);
                default:
                    return null;
            }
        }
    }
}