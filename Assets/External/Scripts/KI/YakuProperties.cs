/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

using System.Collections.Generic;

namespace Hanafuda
{
    public class YakuProperties
    {
        public int ID { get; }
        public Yaku yaku { get; }

        public int InCards
        {
            get
            {
                int cardsLeft = yaku.minSize - Collected;
                if (cardsLeft <= 0)
                {
                    if (yaku.addPoints > 0) return 1;
                    else return 0;
                }
                else return cardsLeft;
            }
        }

        public int Collected;
        public bool Targeted;
        public bool IsPossible;

        public int MinTurns;
        public float Probability;

        public YakuProperties(int id)
        {
            ID = id;
            yaku = Global.allYaku[id];
        }
    }
}