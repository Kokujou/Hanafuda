using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public partial class Main
    {
        public Action ExecutePlaymode
        {
            get
            {
                switch (PlayMode)
                {
                    case 1:
                        return NormalMode;
                    case 2:
                        return DrawMode;
                    default:
                        return () => { };
                }
            }
        }
        private void HoverMatches(string layer)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer(layer))
            {
                Global.prev?.HoverCard(true);
                if (matches.Exists(x => x.name == hit.collider.gameObject.name))
                {
                    selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                    ((BoxCollider)hit.collider).HoverCard();
                }
            }
            else if (hit.collider == null && Global.prev != null)
            {
                selected = null;
                Global.prev.HoverCard(true);
                Board.RefillCards();
            }
        }
        /// <summary>
        /// Aktionen beim ziehen einer Karte
        /// </summary>
        void DrawMode()
        {
            if (Global.MovingCards == 0)
            {
                if (matches.Count == 2)
                {
                    if (Input.GetMouseButton(0) && selected != null)
                    {
                        Card sel = selected;
                        selected = null;
                        Global.prev?.HoverCard(true);
                        Move[2] = Board.Platz.IndexOf(sel);
                        PlayCard(tHandCard, new List<Card>() { sel }, Board.Deck);
                        CheckNewYaku();
                    }
                    else
                        HoverMatches("Feld");
                }
                else
                {
                    selected = null;
                    PlayCard(tHandCard, matches, Board.Deck);
                    CheckNewYaku();
                }
            }
        }
        /// <summary>
        /// Normaler Spielmodus
        /// </summary>
        void NormalMode()
        {
            if (Turn && Global.MovingCards == 0)
            {
                if (Global.Settings.mobile && Global.MovingCards == 0 && Turn && !FieldSelect)
                {
                    if (time == 0f)
                        time = Time.time;
                    else if (Time.time - time > 2)
                    {
                        CreateSlide();
                    }
                    else time = 0f;
                }
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                bool hit = Physics.Raycast(ray, out Hit);
                /*
                 * Bei Auswahl einer hervorgehobenen Karte
                 */
                if ((Input.GetMouseButton(0) || Global.Settings.mobile) && selected != null)
                {
                    GameObject sel = selected.Objekt;
                    selected = null;
                    Global.prev?.HoverCard(true);
                    Card selCard;
                    if (FieldSelect)
                        SelectFieldCard(sel);
                    else
                        MatchCard(sel);
                }
                /*
                 * Mobile Hover-Aktionen
                 */
                else if (Global.Settings.mobile && FieldSelect && hit &&
                    Hit.collider.gameObject.layer == LayerMask.NameToLayer("Feld") && Input.GetMouseButton(0))
                {
                    SelectFieldCard(Hit.collider.gameObject);
                }
                /*
                 * Hover-Aktionen
                 */
                else if (!FieldSelect)
                {
                    Global.prev?.HoverCard(true);
                    StopAllCoroutines();
                    Board.RefillCards();
                    selected = Hit.collider.gameObject.GetComponent<CardRef>().card;
                    ((BoxCollider)Hit.collider).HoverCard();
                    Card selCard = selected;
                    matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
                    for (int i = 0; i < matches.Count; i++)
                    {
                        StartCoroutine(matches[i].BlinkCard());
                    }
                }
                else
                    HoverMatches(FieldSelect ? "Feld" : "P1Hand");
            }
            else if (!Turn && Global.MovingCards == 0)
            {
                if (!Global.Settings.Multiplayer)
                    DrawTurn(((KI)Board.players[Turn ? 0 : 1]).MakeTurn(Board));
            }
        }

        private void MatchCard(GameObject sel)
        {
            Card selCard = sel.GetComponent<CardRef>().card;
            Move[0] = ((Player)Board.players[Turn ? 0 : 1]).Hand.IndexOf(selCard);
            List<Card> Matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
            if (Matches.Count == 2)
            {
                FieldSelect = true;
                tHandCard = selCard;
            }
            else
            {
                PlayCard(selCard, Matches);
                Turn = !Turn;
            }
        }
        private void SelectFieldCard(GameObject sel)
        {
            StopAllCoroutines();
            Board.RefillCards();
            Card selCard = sel.GetComponent<CardRef>().card;
            Move[1] = Board.Platz.IndexOf(selCard);
            PlayCard(tHandCard, new List<Card>() { selCard });
            FieldSelect = false;
            Turn = !Turn;
        }

        private void CreateSlide()
        {
            if (!Slide)
            {
                Slide = Instantiate(Global.prefabCollection.PSlide, Hand1.transform);
                Slide.transform.localPosition = new Vector3(0, -8, 10);
                SlideHand SlideScript = Slide.AddComponent<SlideHand>();
                SlideScript.toHover = Board.Platz;
                SlideScript.cParent = ((Player)Board.players[Turn ? 0 : 1]).Hand;
                SlideScript.onComplete = x => { selected = x; allowInput = true; };
                allowInput = false;
                time = 0f;
            }
        }
    }
}