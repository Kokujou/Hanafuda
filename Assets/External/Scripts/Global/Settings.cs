using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    /// <summary>
    ///     globale Spieleinstellungen
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Anzahl bereits gespielter Runden
        /// </summary>
        public static int Rounds = 0;

        /// <summary>
        ///     Modus der KI: Normal, Schwer, Alptraum
        /// </summary>
        public static int KIMode = 0;

        /// <summary>
        ///     true: 6 Runden, false: 12 Runden
        /// </summary>
        public static bool Rounds6 = true;

        /// <summary>
        ///     true: Mehrspielermodus, false: Einspielermodus
        /// </summary>
        public static bool Multiplayer = false;

        /// <summary>
        ///     Mobiler o. Desktopmodus
        /// </summary>
        public static bool Mobile { get { return Camera.main.aspect < 1; } }

        /// <summary>
        /// Ausgewähltes Hintergrundbild für die Karten
        /// </summary>
        public static int CardSkin = 0;

        /// <summary>
        /// Am Match teilnehmende Spieler
        /// </summary>
        public static List<Player> Players = new List<Player>();

        /// <summary>
        /// ID des aktiven Spielers
        /// </summary>
        public static int PlayerID;

        /// <summary>
        /// Indicates, whether Tutorial Mode is active
        /// </summary>
        public static bool Tutorial;

        /// <summary>
        /// Ruft den Namen des aktuellen Spielers ab
        /// </summary>
        /// <returns></returns>
        public static string GetName()
        {
            return Players[PlayerID].Name;
        }

        /// <summary>
        /// Datum der letzten vom Server erhaltenen Nachricht
        /// </summary>
        public static double LastTime;

        /// <summary>
        /// Gibt einen String zurück, der den Match-Namen im Multiplayer-Modus repräsentiert
        /// </summary>
        /// <returns></returns>
        public static string GetMatchName()
        {
            if (Rounds6)
                return $"6 Rounds | {Players[0].Name}";
            else
                return $"12 Rounds | {Players[0].Name}";
        }
    }
}