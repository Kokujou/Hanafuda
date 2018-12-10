using System;
using System.Collections;
using UnityEngine;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/
/// <summary>
/// veraltet
/// </summary>
namespace Hanafuda
{
    public class CardRef : MonoBehaviour
    {
        public Card card;
    }

    [Serializable]
    public class Card
    {
        public enum Monate
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

        public enum Typen
        {
            Landschaft,
            Bänder,
            Tiere,
            Lichter
        }

        public Material Image;
        public Monate Monat;
        public string Name;
        public GameObject Objekt;
        public Typen Typ;

        public Card(Monate monat, Typen typ, string name, GameObject objekt = null)
        {
            Monat = monat;
            Typ = typ;
            Name = name;
            Objekt = objekt;
            Image = Resources.Load<Material>("Motive/Materials/" + Name);
            if (Objekt)
                Objekt.AddComponent<CardRef>().card = this;
        }

        public IEnumerator BlinkCard()
        {
            var faktor = 1;
            while (true)
            {
                foreach (Transform side in Objekt.transform)
                {
                    var mat = side.gameObject.GetComponent<MeshRenderer>().material;
                    if (side.gameObject.name == "Background")
                    {
                        mat.SetColor("_TintColor", new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        if (mat.GetColor("_TintColor").a + faktor * 0.01f <= 0.5f &&
                            mat.GetColor("_TintColor").a + faktor * 0.01f >= 0)
                            mat.SetColor("_TintColor", mat.GetColor("_TintColor") + new Color(0, 0, 0, faktor * 0.01f));
                        else
                            faktor *= -1;
                    }
                }

                yield return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Card))
            {
                if (((Card) obj).Name == Name)
                    return true;
                return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}