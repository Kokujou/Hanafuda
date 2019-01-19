using System;
using System.Collections.Generic;
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
    public partial class KI : Player
    {
        public enum Mode
        {
            Statistic,
            Searching,
            Omniscient
        }

        public Func<VirtualBoard, Move> MakeTurn;
        public Mode mode;
        public KI(Mode Modus, string name) : base(name)
        {
            thisLock = new object();
            mode = Modus;
            //Thread temp = new Thread(() => BuildStateTree());
            //temp.Start();
            switch (Modus)
            {
                case Mode.Omniscient:
                    /*
                     *  Umsetzung durch Bewertungsfunktion zuerst
                     */
                    MakeTurn = OmniscientCalcTurn;
                    break;
                case Mode.Searching:
                    /*
                     *  - Berechnung der Suchbäume von Spieler und Gegner separat -> 8!*2 statt 8!^2
                     *      -> Suchen des frühesten KoiKoi-Ausrufs
                     *      -> Einschränkung dieser Möglichkeiten basierend auf tatsächlichen Zügen
                     *      -> Mögliche Interaktion der beiden Bäume
                     *  - Problem: Unbekannte Deck-Karten -> Arbeit nur mit Feldkarten möglich
                     *  - Alternative: Zufälliger Zug aus verdeckten Karten -> sehr fehleranfällig!
                     *  - Ehrlichere KI: Aufbau des KI-Baums + Statistische Wahl der Gegnerzüge.
                     *      - Aufbau auch über 2-3 mögliche Züge denkbar
                     */
                    MakeTurn = SearchTurn;
                    break;
                case Mode.Statistic:
                    /*
                     *  Rekonstruierbare Informationen:
                     *  - Inhalt verdeckter Karten = Alle Karten - Hand - Feld - eigene Sammlung - gegnerische Sammlung
                     *      -> statistische Whkt für gegnerische Handkarten und gezogene Folgekarten
                     *  - Gegner: Erreichbare Yaku durch Summe der verdeckten Karten ermitteln
                     *      -> Konzentration auf Vermeidung dieser Zustände
                     *  - Wahrscheinlichkeit eines KoiKoi Ausrufs basierend auf bisheriger Punktelage und Zahl der Handkarten
                     *      - Beachte auch eigene Bedrohungslage an Hand von nahezu komplettierten Yaku
                     *  - Mögliche eigene Yakus und KoiKoi-Möglichkeiten an Hand von eigenen und verdeckten Karten ermitteln
                     *  - Verhindern von gegnerischen KoiKois durch gezieltes "wegschnappen" von Karten
                     *  - Besondere Beachtung vom Einsammeln durch Kartenzug -> Erweiterung der Möglichkeiten der Handkarten
                     *  -> Bewertungsfunktion
                     */
                    MakeTurn = StatisticCalcTurn;
                    break;
            }
        }
        public float StochasticRateState(VirtualBoard State)
        {
            return 0;
        }

        public Move StatisticCalcTurn(VirtualBoard root)
        {
            Move result = new Move();
            return result;
        }

        public Move SearchTurn(VirtualBoard board)
        {
            root = board;
            Move result = new Move();
            BuildStateTree(SkipOpponent:true);
            return result;
        }
    }
}