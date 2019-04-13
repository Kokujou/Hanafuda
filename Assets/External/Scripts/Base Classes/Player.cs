using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

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
    public class Player
    {
        public Dictionary<int, int> CollectedYaku;
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
            CollectedYaku = new Dictionary<int, int>(copy.CollectedYaku);
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
            CollectedYaku = new Dictionary<int, int>();
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                CollectedYaku.Add(yakuID, 0);
            TotalPoints = 0;
        }

        public void Reset()
        {
            Hand.Clear();
            CollectedCards.Clear();
            CollectedYaku.Clear();
            tempPoints = 0;
            Koikoi = 0;
        }

        public void CollectCards(List<Card> NewCards)
        {
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
            {
                Yaku yaku = Global.allYaku[yakuID];
                foreach (Card card in NewCards)
                {
                    if (!yaku.Contains(card)) continue;
                    CollectedYaku[yakuID]++;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}