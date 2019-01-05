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
        private List<KeyValuePair<Yaku, int>> _CollectedYaku;
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
            CollectedYaku = new List<KeyValuePair<Yaku, int>>();
            TotalPoints = 0;
        }
        public List<KeyValuePair<Yaku, int>> CollectedYaku
        {
            get { return _CollectedYaku; }
            set
            {
                _CollectedYaku = value;
                CalcPoints();
            }
        }

        public void Reset()
        {
            Hand.Clear();
            CollectedCards.Clear();
            CollectedYaku.Clear();
            tempPoints = 0;
            Koikoi = 0;
        }

        public void CalcPoints()
        {
            var nPoints = 0;
            Yaku.DistinctYakus(CollectedYaku);
            for (var i = 0; i < CollectedYaku.Count; i++)
            {
                var old = nPoints;
                nPoints += CollectedYaku[i].Key.basePoints;
                if (CollectedYaku[i].Key.addPoints != 0)
                    nPoints += (CollectedCards.Count(x => x.Typ == CollectedYaku[i].Key.TypPref) -
                                CollectedYaku[i].Key.minSize) * CollectedYaku[i].Key.addPoints;
                CollectedYaku[i] = new KeyValuePair<Yaku, int>(CollectedYaku[i].Key, nPoints - old);
            }

            tempPoints = nPoints + Koikoi;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}