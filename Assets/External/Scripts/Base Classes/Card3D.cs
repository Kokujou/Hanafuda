using Hanafuda.Base;
using Hanafuda.Base.Interfaces;
using System;
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

    [Serializable, CreateAssetMenu(menuName = "Card2")]
    public class Card3D : ScriptableObject, ICardObject, ICard
    {
        public string Title { get; set; }
        public int ID { get; set; }
        public Months Month { get; set; }
        public CardMotive Motive { get; set; }

        public Material Image { get; set; }

        GameObject _Objekt;
        public GameObject Object { get { return _Objekt; } set { _Objekt = value; _Objekt.GetComponent<CardComponent>().card = this; } }

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
            if (Settings.Mobile)
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
            if (obj.GetType() == typeof(ICard))
                return ((ICard)obj).Title == Title;
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
            if (obj.GetType() == typeof(ICard))
                return GetHashCode().CompareTo(((ICard)obj).GetHashCode());
            else throw new NotImplementedException();
        }
    }
}