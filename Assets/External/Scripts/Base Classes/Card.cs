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
        public Material Image;
        public Months Monat;
        GameObject _Objekt;
        public GameObject Object { get { return _Objekt; } set { _Objekt = value; _Objekt.GetComponent<CardComponent>().card = this; } }
        public Type Typ;
        public void FadeCard(bool hide = true)
        {
            var mat = Object.GetComponent<CardComponent>().Foreground.GetComponent<MeshRenderer>().material;
            float color = hide ? .2f : .5f;
            mat.SetColor("_TintColor", new Color(color,color,color));
        }
        public void HoverCard(bool unhover = false)
        {
            BoxCollider col = Object.GetComponent<BoxCollider>();
            if (!col) return;
            int factor = unhover ? -1 : 1;
            if (Global.Settings.mobile)
            {
                var tempZ = col.gameObject.transform.position.z;
                col.gameObject.transform.Translate(0, factor * 10, 0);
                col.gameObject.transform.position = new Vector3(col.gameObject.transform.position.x,
                    col.gameObject.transform.position.y, tempZ);
            }
            else
            {
                col.gameObject.transform.position -= factor * new Vector3(0, 0, 5);
                col.gameObject.transform.localScale *= Mathf.Pow(2, factor);
                col.size /= Mathf.Pow(2, factor);
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