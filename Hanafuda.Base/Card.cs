using Hanafuda.Base.Interfaces;
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
    [Serializable]
    public partial class Card : ICard
    {
        public string Title { get; }
        public int ID { get; }
        public Months Month { get; }
        public CardMotive Motive { get; }

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