using System;
using System.Collections.Generic;

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

    public class Card
    {
        public enum Months
        {
            Januar,
            Febraur,
            März,
            April,
            Mai,
            Juni,
            Juli,
            August,
            September,
            Oktober,
            November,
            Dezember,
            Null
        }

        public enum Type
        {
            None = -1,
            Landschaft,
            Bänder,
            Tiere,
            Lichter
        }
        public string Title;
        public int ID;
        public Months Monat;
        public Type Typ;

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Card))
                return ((Card)obj).Title == Title;
            return false;
        }

        public override string ToString()
        {
            return Title;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(Card))
                return GetHashCode().CompareTo(((Card)obj).GetHashCode());
            else throw new NotImplementedException();
        }
    }
}