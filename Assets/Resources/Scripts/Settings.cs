using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        /// <summary>
        ///     globale Spieleinstellungen
        /// </summary>
        public static class Settings
        {
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
            ///     Namen der Spieler
            /// </summary>
            public static string P1Name = "", P2Name = "";

            /// <summary>
            ///     Name des aktiven Spielers
            /// </summary>
            public static string Name = "";

            /// <summary>
            ///     Mobiler o. Desktopmodus
            /// </summary>
            public static bool mobile = false;

            public static List<NetworkClient> playerClients = new List<NetworkClient>();
            public static int CardSkin = 0;
        }
    }
}