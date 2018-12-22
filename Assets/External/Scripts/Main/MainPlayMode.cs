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
                        Board.PlayCard(tHandCard, new List<Card>() { sel }, Board.Deck);
                        CheckNewYaku();
                    }
                    else
                        HoverMatches("Feld");
                }
                else
                {
                    selected = null;
                    Board.PlayCard(tHandCard, matches, Board.Deck);
                    CheckNewYaku();
                }
            }
        }
        /// <summary>
        /// Normaler Spielmodus
        /// </summary>
        void NormalMode()
        {
            if (Board.Turn && Global.MovingCards == 0)
            {
                if (Global.Settings.mobile && Global.MovingCards == 0 && Board.Turn && !FieldSelect)
                {
                    if (time == 0f)
                        time = Time.time;
                    else if (Time.time - time > 2)
                        CreateSlide();
                }
                else time = 0f;
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
                    StopAllCoroutines();
                    Board.RefillCards();
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
                else
                    HoverMatches(FieldSelect ? "Feld" : "P1Hand", () =>
                    {
                        if (!FieldSelect)
                        {
                            Global.prev?.HoverCard(true);
                            ((BoxCollider)Hit.collider)?.HoverCard();
                            if (!selected || selected.Title != Hit.collider.name)
                            {
                                StopAllCoroutines();
                                Board.RefillCards();
                                selected = Hit.collider.gameObject.GetComponent<CardRef>().card;
                                matches = Board.Platz.FindAll(x => x.Monat == selected.Monat);
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    StartCoroutine(matches[i].BlinkCard());
                                }
                            }
                        }
                    });
            }
            else if (!Board.Turn && Global.MovingCards == 0)
            {
                if (!Global.Settings.Multiplayer)
                    ((KI)Board.players[Board.Turn ? 0 : 1]).MakeTurn(Board).Apply();
            }
        }
        private void HoverMatches(string layer, Action insert = null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer(layer))
            {
                Global.prev?.HoverCard(true);
                insert?.Invoke();
                if (matches.Exists(x => x.Title == hit.collider.gameObject.name))
                {
                    selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                    ((BoxCollider)hit.collider)?.HoverCard();
                }
            }
            else if (hit.collider == null && Global.prev != null)
            {
                selected = null;
                Global.prev?.HoverCard(true);
                Board.RefillCards();
            }
        }
        private void MatchCard(GameObject sel)
        {
            Card selCard = sel.GetComponent<CardRef>().card;
            Move[0] = ((Player)Board.players[Board.Turn ? 0 : 1]).Hand.IndexOf(selCard);
            List<Card> Matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
            if (Matches.Count == 2)
            {
                FieldSelect = true;
                tHandCard = selCard;
            }
            else
            {
                Board.PlayCard(selCard, Matches);
                Board.Turn = !Board.Turn;
            }
        }
        private void SelectFieldCard(GameObject sel)
        {
            StopAllCoroutines();
            Board.RefillCards();
            Card selCard = sel.GetComponent<CardRef>().card;
            Move[1] = Board.Platz.IndexOf(selCard);
            Board.PlayCard(tHandCard, new List<Card>() { selCard });
            FieldSelect = false;
            Board.Turn = !Board.Turn;
        }

        private void CreateSlide()
        {
            if (!Slide)
            {
                Slide = Instantiate(Global.prefabCollection.PSlide);
                Slide.transform.localPosition = new Vector3(0, -8, 10);
                SlideHand SlideScript = Slide.AddComponent<SlideHand>();
                SlideScript.toHover = Board.Platz;
                SlideScript.cParent = ((Player)Board.players[Board.Turn ? 0 : 1]).Hand;
                SlideScript.onComplete = x => { selected = x; allowInput = true; };
                allowInput = false;
                time = 0f;
            }
        }
    }
}