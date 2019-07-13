using System.Collections.Generic;
using System.Linq;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

namespace Hanafuda.Base
{
    public class Player
    {
        public List<Card> CollectedCards;
        public List<Card> Hand;
        public int Koikoi;
        public string Name;
        public List<int> pTotalPoints = new List<int>();
        public int tempPoints;
        public int TotalPoints;
        public int LastResponse;

        /// <summary>
        /// Konstruktor nicht für Mehrspielermodus geeignet
        /// </summary>
        /// <param name="copy">Originalspieler</param>
        public Player(Player copy)
        {
            CollectedCards = new List<Card>(copy.CollectedCards);
            Hand = new List<Card>(copy.Hand);
            Koikoi = copy.Koikoi;
            Name = copy.Name;
            pTotalPoints = new List<int>(copy.pTotalPoints);
            tempPoints = copy.tempPoints;
            TotalPoints = copy.TotalPoints;
        }

        public Player(string name)
        {
            Name = name;
            Koikoi = 0;
            Hand = new List<Card>();
            CollectedCards = new List<Card>();
            TotalPoints = 0;
        }

        public void Reset()
        {
            Hand.Clear();
            CollectedCards.Clear();
            tempPoints = 0;
            Koikoi = 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}