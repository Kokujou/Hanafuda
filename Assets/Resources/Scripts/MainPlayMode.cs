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
        public Action Execute
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
                switch (matches.Count)
                {
                    case 0:
                    case 1:
                    case 3:
                        selected = null;
                        PlayCard(tHandCard, matches, Board.Deck);
                        goto default;
                    case 2:
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Input.GetMouseButton(0) && selected != null)
                        {
                            Card sel = selected;
                            selected = null;
                            if (Global.prev != null)
                            {
                                Global.prev.HoverCard(true);
                                Global.prev = null;
                            }
                            Move[2] = Board.Platz.IndexOf(sel);
                            PlayCard(tHandCard, new List<Card>() { sel }, Board.Deck);
                            goto default;
                        }
                        else if (Physics.Raycast(ray, out hit, 5000f, 1 << LayerMask.NameToLayer("Feld")) && (selected == null || selected.Objekt != hit.collider.gameObject))
                        {
                            if (Global.prev != null)
                            {
                                selected = null;
                                Global.prev.HoverCard(true);
                                Global.prev = null;
                            }
                            if (matches.Exists(x => x.name == hit.collider.gameObject.name))
                            {
                                selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                                ((BoxCollider)hit.collider).HoverCard();
                            }
                        }
                        else if (hit.collider == null && Global.prev != null)
                        {
                            if (Global.prev != null)
                            {
                                selected = null;
                                Global.prev.HoverCard(true);
                                Global.prev = null;
                            }
                            Board.RefillCards();
                        }
                        break;
                    default:
                        int oPoints = ((Player)(Board.players[Turn ? 0 : 1])).tempPoints;
                        List<KeyValuePair<Yaku, int>> oYaku = ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku;
                        ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku = new List<KeyValuePair<Yaku, int>>(Yaku.GetYaku(((Player)(Board.players[Turn ? 0 : 1])).CollectedCards).ToDictionary(x => x, x => 0));
                        newYaku.Clear();
                        if (((Player)(Board.players[Turn ? 0 : 1])).tempPoints > oPoints)
                        {
                            for (int i = 0; i < ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku.Count; i++)
                            {
                                if (!oYaku.Exists(x => x.Key.name == ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku[i].Key.name && x.Value == ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku[i].Value))
                                {
                                    newYaku.Add(((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku[i].Key);
                                }
                            }
                            _CherryBlossoms = Instantiate(Global.prefabCollection.CherryBlossoms);
                        }
                        if (newYaku.Count == 0)
                        {
                            _Turn = !_Turn;
                            PlayMode = 1;
                            if (Global.Settings.Multiplayer)
                            {
                                string move = Move[0].ToString() + "," + Move[1].ToString() + "," + Move[2].ToString();
                                if (NetworkServer.active)
                                    NetworkServer.SendToAll(131, new Global.Message() { message = move });
                                else
                                    Global.Settings.playerClients[0].Send(131, new Global.Message() { message = move });
                            }
                        }
                        Move = new[] { -1, -1, -1 };
                        break;
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
                if (Global.MovingCards == 0 && Turn && !FieldSelect)
                {
                    if (time == 0f)
                        time = Time.time;
                    else if (Time.time - time > 2)
                    {
                        if (Global.Settings.mobile && !Slide)
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
                else time = 0f;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                /*
                 * Bei Auswahl einer hervorgehobenen Karte
                 */
                if ((Input.GetMouseButton(0) || Global.Settings.mobile) && selected != null)
                {
                    GameObject sel = selected.Objekt;
                    selected = null;
                    if (Global.prev != null)
                    {
                        Global.prev.HoverCard(true);
                        Global.prev = null;
                    }
                    Card selCard;
                    /*
                     * Auswahl bei zwei passenden Karten in den Feldkarten
                     */
                    if (FieldSelect)
                    {
                        StopAllCoroutines();
                        Board.RefillCards();
                        selCard = sel.GetComponent<CardRef>().card;
                        Move[1] = Board.Platz.IndexOf(selCard);
                        PlayCard(tHandCard, new List<Card>() { selCard });
                        FieldSelect = false;
                        Turn = !Turn;
                    }
                    /*
                     * Bei normaler Auswahl:
                     *  - Einsammeln bezüglich passender Karten und anschließender Rundenwechsel
                     *  - Bei 2 übereinstimmenden Karten: Warten auf Auswahl des Spielers
                     */
                    else
                    {
                        selCard = sel.GetComponent<CardRef>().card;
                        Move[0] = ((Player)Board.players[Turn ? 0 : 1]).Hand.IndexOf(selCard);
                        List<Card> Matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
                        switch (Matches.Count)
                        {
                            case 0:
                                PlayCard(selCard);
                                Turn = !Turn;
                                break;
                            case 1:
                            case 3:
                                PlayCard(selCard, Matches);
                                Turn = !Turn;
                                break;
                            case 2:
                                FieldSelect = true;
                                tHandCard = selCard;
                                break;
                        }
                    }
                }
                /*
                 * Mobile Hover-Aktionen
                 */
                else if (Global.Settings.mobile && FieldSelect && Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Feld") && Input.GetMouseButton(0))
                {
                    StopAllCoroutines();
                    Board.RefillCards();
                    Card selCard = hit.collider.gameObject.GetComponent<CardRef>().card;
                    Move[1] = Board.Platz.IndexOf(selCard);
                    PlayCard(tHandCard, new List<Card>() { selCard });
                    FieldSelect = false;
                    Turn = !Turn;
                }
                /*
                 * Hover-Aktionen
                 */
                else if (Physics.Raycast(ray, out hit, 5000f, FieldSelect ? 1 << LayerMask.NameToLayer("Feld") : 1 << LayerMask.NameToLayer("P1Hand"))
                    && (selected == null || selected.Objekt != hit.collider.gameObject))
                {
                    if (Global.prev != null)
                    {
                        Global.prev.HoverCard(true);
                        Global.prev = null;
                    }
                    if (!FieldSelect)
                    {
                        StopAllCoroutines();
                        Board.RefillCards();
                        selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                        ((BoxCollider)hit.collider).HoverCard();
                        Card selCard = selected;
                        matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            StartCoroutine(matches[i].BlinkCard());
                        }
                    }
                    else if (matches.Exists(x => x.name == hit.collider.gameObject.name))
                    {
                        selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                        ((BoxCollider)hit.collider).HoverCard();
                    }
                }
                /*
                 * Hover-Zurücksetzung onLeave
                 */
                else if (hit.collider == null && Global.prev != null)
                {
                    if (Global.prev != null)
                    {
                        selected = null;
                        Global.prev.HoverCard(true);
                        Global.prev = null;
                    }
                    Board.RefillCards();
                }
            }
            else if (!Turn && Global.MovingCards == 0)
            {
                /*
                 * Gegnerischer Zug: Annähernd identisch, lediglich Auswahl der Karte über KI
                 * Sonderfall: zwei übereinstimmende Karten -> Geregelt über Ausgabe der KI
                 */
                if (Global.Settings.Multiplayer)
                    return;
                else
                    DrawTurn(((KI)Board.players[Turn ? 0 : 1]).MakeTurn(Board));

            }
        }
    }
}