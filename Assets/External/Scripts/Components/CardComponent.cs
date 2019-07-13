
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
    public class CardComponent : MonoBehaviour
    {
        public ICard card;
        public GameObject Foreground;
        public GameObject Background;
        private Action CardInteraction = () => { };
        private void HandInteraction()
        {

        }
        private void FieldInteraction()
        {

        }
        private void Update()
        {
            CardInteraction();
        }
        public void SetActive(bool isField)
        {
            if (isField)
                CardInteraction = FieldInteraction;
            else
                CardInteraction = HandInteraction;
        }
    }
}