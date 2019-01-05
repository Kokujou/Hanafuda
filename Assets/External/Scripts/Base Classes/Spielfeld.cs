using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public partial class Spielfeld : MonoBehaviour
    {
        public List<Card> Deck = new List<Card>();
        public List<Card> Field = new List<Card>();
        public List<Player> players = new List<Player>();
        public bool _Turn = true;
        public bool Turn
        {
            get { return _Turn; }
            set
            {
                _Turn = value;
            }
        }
    }
}