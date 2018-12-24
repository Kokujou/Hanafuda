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
    public class PlayerComponent : MonoBehaviour
    {
        public Player player;
        private Spielfeld Board;
        private Action InputRoutine;
        private bool isActive;
        private GameObject Slide;
        private Card Selection;
        public void Activate(bool active)
        {
            isActive = active;
        }
        public void Awake()
        {
            Board = gameObject.GetComponent<Spielfeld>();
            InputRoutine = HandInteraction;
        }
        public void Init(Player reference)
        {
            player = reference;
            isActive = true;
        }
        public void Update()
        {
            if (Board.Turn && isActive && Global.MovingCards == 0)
                InputRoutine();
        }
        public void HandInteraction()
        {
            if (Global.Settings.mobile)
                CreateSlide();
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("P1Hand")))
                {
                    Card selected = hit.collider.gameObject.GetComponent<CardComponent>().card;
                    if (Input.GetMouseButtonDown(0))
                        Board.SelectCard(selected);
                    else
                        Board.HoverHand(selected);
                }
                else
                    Board.HoverHand(null);
            }
        }
        private void CreateSlide()
        {
            Slide = Instantiate(Global.prefabCollection.PSlide, MainSceneVariables.variableCollection.Hand1M);
            Slide.transform.localPosition = new Vector3(0, -8, 10);
            SlideHand SlideScript = Slide.AddComponent<SlideHand>();
            SlideScript.Init(player.Hand.Count,
                x => Board.HoverHand(x >= 0 ? player.Hand[x] : null),
                x => Board.SelectCard(x >= 0 ? player.Hand[x] : null));
            //SlideScript.onSelect = x => { Selection = x; Activate(true); };
            Activate(false);
        }

        public void FieldInteraction()
        {

        }

        public void RequestFieldSelection()
        {
            InputRoutine = FieldInteraction;
            isActive = true;
        }

    }
}