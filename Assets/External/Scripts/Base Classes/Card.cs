using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

    [Serializable, CreateAssetMenu(menuName = "Card")]
    public class Card : ScriptableObject
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
            None = -1,
            Landschaft,
            Bänder,
            Tiere,
            Lichter
        }
        public string Title;
        public Material Image;
        public Monate Monat;
        GameObject _Objekt;
        public GameObject Objekt { get { return _Objekt; } set { _Objekt = value; _Objekt.AddComponent<CardRef>().card = this; } }
        public Typen Typ;
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
                return ((Card)obj).Title == Title;
            return false;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}