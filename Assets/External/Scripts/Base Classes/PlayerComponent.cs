﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Player> Players;
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
        public void Init(List<object> players)
        {
            Players = new List<Player>();
            for (int i = 0; i < players.Count; i++)
                Players.Add((Player)players[0]);
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
            SlideScript.Init(Players[0].Hand.Count,
                x => Board.HoverHand(x >= 0 ? Players[0].Hand[x] : null),
                x => Board.SelectCard(x >= 0 ? Players[0].Hand[x] : null));
            //SlideScript.onSelect = x => { Selection = x; Activate(true); };
            Activate(false);
        }

        public void FieldInteraction(Card card, bool fromDeck)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("Feld")) && 
                hit.collider.gameObject.name != card.Title)
            {
                Card selected = hit.collider.gameObject.GetComponent<CardComponent>().card;
                Board.SelectCard(selected, fromDeck);
            }

        }

        public void RequestFieldSelection(Card card, bool fromDeck)
        {
            InputRoutine = () => { FieldInteraction(card, fromDeck); };
            isActive = true;
        }

    }
}