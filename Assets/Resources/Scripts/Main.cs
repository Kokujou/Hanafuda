using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/*
 * Todo-Liste:
 * - Künstliche Intelligenz
 * - Cast<object/Player> überdenken
 * - Feature: Spielverlauf visualisieren
 * - Feature: Sammlungen beim Rundenende anzeigen
 * - (Partikel-Animationen für Yaku) (einbinden)
 * - (Animation für Rundenende und Spielende) (einbinden)
 * - Multiplayer
 *      - Problem beim Arrangieren des Karteneinsammelns, in falsche Sammlung geordnet
 *      - Kompatibilität mit Android-Modus testen
 * - Layout für Android-Modus
 *      - Einsammel-Animation überdenken
 *      - Größen überdenken
 * - NewYaku Animationen überspringen
 * - Yaku-Übermittlung von Spieler 2
 * - Feature: Zuschauer
 * - Achtung! Neu mischen wenn 4 gleiche Feldkarten!
 * - Sammlungs-UI Um gegnerische Karten erweitern
 * - Win/Loose-Animation
 * - Optional: Komplette Codeüberarbeitung zwecks Trennung von Grafik und Hintergrundprozessen
 * - Idee: Tutorial-Stage
 * - Kartenfächerung für wenige Karten überarbeiten
 * - Yaku erst nach Deck-Zug anzeigen
 */

namespace Hanafuda
{
    /// <summary>
    /// Klasse für die Spielmechaniken im Einzelspielermodus
    /// </summary>
    public partial class Main : NetworkBehaviour
    {
        // Use this for initialization
        public GUISkin Skin;
        public bool allowInput = true;
        internal GameObject Slide, _CherryBlossoms;
        Transform EffectCam, Hand1, Hand2, Platz, Deck;
        public Vector3 test;
        readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        internal Spielfeld Board;
        KI Opponent;
        List<Card> matches = new List<Card>();
        Card tHandCard;
        internal List<Yaku> newYaku = new List<Yaku>();
        float time, animLeft = -200;
        bool FieldSelect, _Turn = true, initKoikoi, shownYaku;
        /// <summary>
        /// 1: Normal, 2: Kartenzug
        /// </summary>
        internal int PlayMode;
        /// <summary>
        /// 0: Gespielte Karte, 1: Gewählte Feldkarte (falls nötig), 2: zusätzliche gewählte Feldkarte bei Ziehen
        /// </summary>
        /// 
        int[] Move = new int[] { -1, -1, -1 };
        /// <summary>
        /// Vereinbarung von Aktionen beim Rundenwechsel
        /// </summary>
        internal bool Turn
        {
            get { return _Turn; }
            set
            {
                if (PlayMode == 1 && _Turn)
                {
                    // Einsammeln bei Kartenzug? //
                    matches = new List<Card>();
                    if (Board.Platz.Exists(x => x.Monat == Board.Deck[0].Monat))
                        matches.AddRange(Board.Platz.FindAll(x => x.Monat == Board.Deck[0].Monat));
                    selected = Board.Deck[0];
                    tHandCard = Board.Deck[0];
                    if (matches.Count == 2)
                    {
                        if (Global.Settings.mobile)
                        {
                            for (int i = 0; i < Board.Platz.Count; i++)
                            {
                                Color col = matches.Exists(x => x.Name == Board.Platz[i].Name) ? new Color(.3f, .3f, .3f) : new Color(.5f, .5f, .5f);
                                Board.Platz[i].Objekt.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Foreground").material.SetColor("_TintColor", col);
                            }
                        }
                        else
                            for (int i = 0; i < 2; i++)
                                StartCoroutine(matches[i].BlinkCard());
                    }
                    PlayMode = 2;
                }
                else _Turn = value;
            }
        }
        void SyncDeck(NetworkMessage msg)
        {
            int seed = Convert.ToInt32(msg.ReadMessage<Global.Message>().message);
            GenerateDeck(seed);
        }
        void SyncMove(NetworkMessage msg)
        {
            string[] splitted = msg.ReadMessage<Global.Message>().message.Split(',');
            int[] move = new int[3];
            for (int i = 0; i < splitted.Length && i < 3; i++)
                move[i] = Convert.ToInt32(splitted[i]);
            Debug.Log(move[0] + "," + move[1] + "," + move[2]);
            DrawTurn(move);
        }
        void GenerateDeck(int seed)
        {
            if (Global.players.Count == 0)
                Board = new Spielfeld(new List<object>() { new Player(Global.Settings.P1Name), new Player(Global.Settings.P2Name) }, seed);
            else
            {
                Board = new Spielfeld(Global.players.Cast<object>().ToList(), seed);
                for (int i = 0; i < Board.players.Count; i++)
                    ((Player)Board.players[0]).Reset();
            }
            _Turn = Global.Settings.Name == ((Player)Board.players[Global.Turn]).Name;
            FieldSetup();
        }
        void Awake()
        {
            selected = null;
            EffectCam = MainSceneVariables.variableCollection.EffectCamera;
            if (Global.Settings.mobile)
            {
                Hand1 = MainSceneVariables.variableCollection.Hand1M;
                Hand2 = MainSceneVariables.variableCollection.Hand2M;
                Platz = MainSceneVariables.variableCollection.MFeld;
                Deck = MainSceneVariables.variableCollection.MDeck;
            }
            else
            {
                MainSceneVariables.variableCollection.ExCol.gameObject.SetActive(false);
                Hand1 = MainSceneVariables.variableCollection.Hand1;
                Hand2 = MainSceneVariables.variableCollection.Hand2;
                Platz = MainSceneVariables.variableCollection.Feld;
                Deck = MainSceneVariables.variableCollection.Deck;
            }
            PlayMode = 1;
            if (Global.Settings.Multiplayer)
            {
                NetworkServer.RegisterHandler(131, SyncMove);
                for (int i = 0; i < Global.Settings.playerClients.Count; i++)
                {
                    Global.Settings.playerClients[i].RegisterHandler(131, SyncMove);
                    Global.Settings.playerClients[i].RegisterHandler(132, SyncDeck);
                }
                if (NetworkServer.active)
                {
                    int seed = UnityEngine.Random.Range(0, 1000);
                    NetworkServer.SendToAll(132, new Global.Message() { message = seed.ToString() });
                    GenerateDeck(seed);
                }
                return;
            }
            if (Global.players.Count == 0)
                Board = new Spielfeld(new List<object>() { new Player(Global.Settings.P1Name), new KI((KI.Mode)Global.Settings.KIMode, Board, Turn, "Computer") });
            else
            {
                Board = new Spielfeld(Global.players.Cast<object>().ToList());
                for (int i = 0; i < Board.players.Count; i++)
                    ((Player)Board.players[0]).Reset();
            }
            FieldSetup();
        }
        /// <summary>
        /// Erstellung des Decks, sowie Austeilen von Händen und Spielfeld
        /// </summary>
        void FieldSetup()
        {
            for (int i = 0; i < Board.Deck.Count; i++)
            {
                GameObject temp = Instantiate(Global.prefabCollection.PKarte);
                temp.name = Board.Deck[i].Name;
                temp.GetComponentsInChildren<MeshRenderer>()[0].material = Board.Deck[i].Image;
                temp.transform.parent = Deck.transform;
                temp.transform.localPosition = new Vector3(0, 0, i * 0.015f);
                Board.Deck[i] = new Card(Board.Deck[i].Monat, Board.Deck[i].Typ, Board.Deck[i].Name, temp);
            }
            for (int i = Global.Settings.Name == Global.Settings.P1Name ? 0 : 1; Global.Settings.Name == Global.Settings.P1Name ? i < 2 : i >= 0; i += (Global.Settings.Name == Global.Settings.P1Name ? 1 : -1))
                for (int j = 0; j < 8; j++)
                {
                    ((Player)Board.players[i]).Hand.Add(Board.Deck[0]);
                    GameObject temp = Board.Deck[0].Objekt;
                    Board.Deck.RemoveAt(0);
                    temp.transform.parent = i == 0 ? Hand1.transform : Hand2.transform;
                    if (!Global.Settings.mobile)
                        temp.layer = LayerMask.NameToLayer("P" + (i + 1).ToString() + "Hand");
                    /* Zugedeckte Transformation mit anschließender Aufdeckrotation */
                    if (Global.Settings.mobile)
                    {
                        StartCoroutine(Global.StandardAnimation(temp.transform, temp.transform.parent.position + new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), -j), new Vector3(0, 0, (i == 0 ? 0 : 180) + UnityEngine.Random.Range(-60, 60)), temp.transform.localScale, (j + 8 * i) * 0.2f, .3f, () => { temp.transform.position += new Vector3(0, 0, 1); }));
                        StartCoroutine(Global.StandardAnimation(temp.transform, temp.transform.parent.position, new Vector3(0, 0, (i == 0 ? 0 : 180)), temp.transform.localScale, 18 * 0.2f, .3f, () =>
                        {
                            GameObject card = new GameObject();
                            card.transform.parent = temp.transform.parent;
                            temp.transform.parent = card.transform;
                            bool hand1 = temp.transform.parent.parent.name.Contains("1");
                            card.transform.localPosition = new Vector3(0, hand1 ? -8 : 8);
                            temp.transform.localPosition = new Vector3(0, hand1 ? 8 : -8, 0);
                            List<Transform> hand = new List<Transform>(temp.transform.parent.parent.gameObject.GetComponentsInChildren<Transform>());
                            hand.RemoveAll(x => !x.name.Contains("New"));
                            int id = hand.IndexOf(temp.transform.parent);
                            StartCoroutine(Global.StandardAnimation(temp.transform.parent, temp.transform.parent.position + new Vector3(0, 0, id), temp.transform.parent.eulerAngles + new Vector3(0, 0, -60f + (120f / 7) * id), temp.transform.parent.localScale, .6f, .3f, () =>
                                  {
                                      GameObject oldParent = temp.transform.parent.gameObject;
                                      temp.transform.parent = temp.transform.parent.parent;
                                      Destroy(oldParent);
                                      //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, id / 10f);
                                  }));
                        }));
                        StartCoroutine(Global.StandardAnimation(Hand1.transform, Hand1.transform.position, new Vector3(0, 180, 0), Hand1.transform.localScale, 5f));
                    }
                    else
                    {
                        StartCoroutine(Global.StandardAnimation(temp.transform, temp.transform.parent.position + new Vector3((j) * 11f, 0, -j), Vector3.zero, temp.transform.localScale, (j + 8 * i) * 0.2f));
                        StartCoroutine(Global.StandardAnimation(temp.transform, temp.transform.parent.position + new Vector3((j) * 11f, 0, -j),
                            temp.transform.parent == Hand1.transform ? new Vector3(0, 180, 0) : Vector3.zero, temp.transform.localScale, 18 * 0.2f));
                    }
                }
            for (int i = 0; i < 8; i++)
            {
                Board.Platz.Add(Board.Deck[0]);
                GameObject temp = Board.Deck[0].Objekt;
                Board.Deck.RemoveAt(0);
                temp.layer = LayerMask.NameToLayer("Feld");
                temp.transform.parent = Platz.transform;
                if (Global.Settings.mobile)
                    StartCoroutine(Global.StandardAnimation(temp.transform, Platz.transform.position + new Vector3(((i) / 3) * (11f / 1.5f), -9 + (18f / 1.5f) * (i % 3), -i), new Vector3(0, 180, 0), temp.transform.localScale / 1.5f, (i + 18) * 0.2f));
                else
                    StartCoroutine(Global.StandardAnimation(temp.transform, Platz.transform.position + new Vector3((i / 2) * 11f, -9 + 18 * (((i) + 1) % 2), -i), new Vector3(0, 180, 0), temp.transform.localScale, (i + 18) * 0.2f));
                //StartCoroutine(Global.StandardAnimation(temp.transform, GameObject.Find("Feld").transform.position + new Vector3((int)(i/2), 0, 0), new Vector3(0, 180 * (1 - i), 0), temp.transform.localScale, 16 * 0.2f));
            }
        }

        // Update is called once per frame
        public Card selected;
        /// <summary>
        /// Sortieren von Eltern-Objekten der Karten-Sammlungen
        /// </summary>
        /// <param name="toSort">zu sortierende Sammlung</param>
        /// <param name="StartPos">Startposition der Sammlung</param>
        /// <param name="rows">Anzahl der Zeilen, auf die die Karten aufgeteilt werden sollen</param>
        /// <param name="maxCols">Maximale Anzahl von Spalten</param>
        /// <returns></returns>
        /// 
        IEnumerator ResortCards(List<Card> toSort, Vector3 StartPos, int rows = 1, int maxCols = int.MaxValue, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            while ((float)toSort.Count / rows > maxCols)
                rows++;
            for (int i = 0; i < toSort.Count; i++)
            {
                StartCoroutine(Global.StandardAnimation(toSort[i].Objekt.transform, StartPos +
                    new Vector3((i / rows) * 11f, -9 * (rows - 1) + ((i + 1) % rows) * 18, 0),
                    toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
            }
        }
        IEnumerator ResortCardsMobile(List<Card> toSort, Vector3 StartPos, int maxSize, bool isHand = false, bool rowWise = true, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            int iterations = 1;
            if (isHand)
            {
                for (int card = 0; card < toSort.Count; card++)
                {
                    GameObject temp = toSort[card].Objekt;
                    bool hand1 = temp.transform.parent.name.Contains("1");
                    StartCoroutine(Global.StandardAnimation(temp.transform, temp.transform.parent.position, new Vector3(0, temp.transform.rotation.eulerAngles.y, hand1 ? 0 : 180), temp.transform.localScale, 0, .3f, () =>
                         {
                             GameObject Card = new GameObject();
                             Card.transform.parent = temp.transform.parent;
                             temp.transform.parent = Card.transform;
                             Card.transform.localPosition = new Vector3(0, hand1 ? -8 : 8);
                             temp.transform.localPosition = new Vector3(0, hand1 ? 8 : -8, 0);
                             List<Transform> hand = new List<Transform>(temp.transform.parent.parent.gameObject.GetComponentsInChildren<Transform>());
                             hand.RemoveAll(x => !x.name.Contains("New"));
                             int id = hand.IndexOf(temp.transform.parent);
                             float max = ((Player)Board.players[Turn ? 0 : 1]).Hand.Count - 1;
                             if (max == 0) max = 0.5f;
                             StartCoroutine(Global.StandardAnimation(temp.transform.parent, temp.transform.parent.position + new Vector3(0, 0, -id), temp.transform.parent.eulerAngles + new Vector3(0, 0, -60f + (120f / max) * (max - id)), temp.transform.parent.localScale, .6f, .3f, () =>
                                       {
                                           GameObject oldParent = temp.transform.parent.gameObject;
                                           temp.transform.parent = temp.transform.parent.parent;
                                           Destroy(oldParent);
                                           //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, id/10f);
                                       }));
                         }));
                }
            }
            else
            {
                if (rowWise)
                {
                    iterations = maxSize;
                    for (int i = 0; i < toSort.Count; i++)
                    {
                        StartCoroutine(Global.StandardAnimation(toSort[i].Objekt.transform, StartPos +
                            new Vector3((i % 1) * (11f / 1.5f), -9 + (i / 1) * (18f / 1.5f), 0),
                            toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                    }
                }
                else
                {
                    iterations = maxSize;
                    for (int i = 0; i < toSort.Count; i++)
                    {
                        StartCoroutine(Global.StandardAnimation(toSort[i].Objekt.transform, StartPos +
                            new Vector3((i / 1) * (11f / 1.5f), -9 + (i % 1) * (18f / 1.5f), 0),
                            toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                    }
                }
            }

        }
        /// <summary>
        /// Animation des KoiKoi Schriftzugs (Vergrößerung)
        /// </summary>
        /// <param name="append">Aktion bei Vollendung der Animation</param>
        /// <returns></returns>

        public void PlayCard(Card handCard, List<Card> Matches = null, List<Card> source = null)
        {
            bool fromHand = true;
            if (Matches == null) Matches = new List<Card>();
            if (source == null) source = ((Player)Board.players[Turn ? 0 : 1]).Hand;
            else fromHand = false;
            string action = " und sammelt ";
            int j = 0;
            for (j = 0; j < Matches.Count; j++)
            {
                action += Matches[j].Name + ", ";
            }
            if (j == 0) action = " und legt sie aufs Spielfeld.";
            else action = action.Remove(action.Length - 2, 1) + "ein.";
            Global.Spielverlauf.Add(Global.Settings.Name + " wählt " + handCard.Name + action);
            StopAllCoroutines();
            Board.RefillCards();
            source.Remove(handCard);
            handCard.Objekt.transform.parent = Platz.transform;
            handCard.Objekt.layer = LayerMask.NameToLayer("Feld");
            if (Matches.Count > 0)
                Matches.Add(handCard);
            else
            {
                Board.Platz.Add(handCard);
                /*if (mobile)
                {
                    StartCoroutine(ResortCardsMobile(((Player)Board.players[Turn ? 0 : 1]).Hand, (Turn ? Hand1 : Hand2).transform.position, 8, isHand: true, delay: 1));
                    StartCoroutine(ResortCardsMobile(Board.Platz, Platz.transform.position, 3, rowWise: false, delay: 1));
                }
                else
                {
                    StartCoroutine(ResortCards(((Player)Board.players[Turn ? 0 : 1]).Hand, (Turn ? Hand1 : Hand2).transform.position, delay: 1));
                    StartCoroutine(ResortCards(Board.Platz, Platz.transform.position, 2, delay: 1));
                }*/
                StartCoroutine(Global.StandardAnimation(handCard.Objekt.transform, Platz.transform.position +
                    (Global.Settings.mobile ? new Vector3(((Board.Platz.Count - 1) / 3) * (11f / 1.5f), -9 + (18f / 1.5f) * ((Board.Platz.Count - 1) % 3))
                    : new Vector3(((Board.Platz.Count - 1) / 2) * 11f, -9 + 18 * ((Board.Platz.Count - 1) % 2))),
                    new Vector3(0, 180, 0), handCard.Objekt.transform.localScale / (Global.Settings.mobile ? 1.5f : 1f)));
            }
            for (int i = 0; i < matches.Count; i++)
            {
                if (Global.Settings.mobile)
                {
                    GameObject match = matches[i].Objekt;
                    ((Player)Board.players[Turn ? 0 : 1]).CollectedCards.Add(matches[i]);
                    if (i < matches.Count - 1)
                        Board.Platz.Remove(matches[i]);
                    StartCoroutine(Global.StandardAnimation(matches[i].Objekt.transform, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Turn ? 0 : Screen.height)),
                        Vector3.zero, Vector3.zero, 0, AddFunc: () => { Destroy(match); }));
                }
                else
                {
                    Transform collection = MainSceneVariables.variableCollection.PCCollections[
                        ("Collection" + (Turn ? "1" : "2") + matches[i].Typ.ToString()).GetHashCode()];
                    matches[i].Objekt.transform.parent = collection;
                    ((Player)Board.players[Turn ? 0 : 1]).CollectedCards.Add(matches[i]);
                    if (i < matches.Count - 1)
                        Board.Platz.Remove(matches[i]);
                    matches[i].Objekt.layer = LayerMask.NameToLayer("Collected");
                    StartCoroutine(Global.StandardAnimation(matches[i].Objekt.transform, matches[i].Objekt.transform.parent.position +
                        new Vector3(5.5f * ((collection.childCount - 1) % 5), -((collection.childCount - 1) / 5) * 2f, -((collection.childCount - 1) / 5)),
                        new Vector3(0, 180, 0), matches[i].Objekt.transform.localScale / 2));
                }
            }
            if (Global.Settings.mobile)
            {
                if (fromHand)
                    StartCoroutine(ResortCardsMobile(((Player)Board.players[Turn ? 0 : 1]).Hand, (Turn ? Hand1 : Hand2).transform.position, 8, isHand: true, delay: 1));
                StartCoroutine(ResortCardsMobile(Board.Platz, Platz.transform.position, 3, rowWise: false, delay: 1));
            }
            else
            {
                if (fromHand)
                    StartCoroutine(ResortCards(((Player)Board.players[Turn ? 0 : 1]).Hand, (Turn ? Hand1 : Hand2).transform.position, delay: 1));
                StartCoroutine(ResortCards(Board.Platz, Platz.transform.position, 2, delay: 1));
            }
        }
        public void DrawTurn(int[] move)
        {
            Card hCard = ((Player)Board.players[Turn ? 0 : 1]).Hand[move[0]];
            Card fCard = move[1] >= 0 ? Board.Platz[move[1]] : null;
            List<Card> Matches = Board.Platz.FindAll(x => x.Monat == hCard.Monat);
            switch (Matches.Count)
            {
                case 0:
                    PlayCard(hCard);
                    break;
                case 1:
                case 3:
                    PlayCard(hCard, Matches);
                    break;
                case 2:
                    PlayCard(hCard, new List<Card>() { fCard });
                    break;
            }
            Turn = !Turn;
        }
        void Update()
        {
            Global.SetCameraRect(Camera.main);
            //Global.SetCameraRect(EffectCam.GetComponent<Camera>());
            if (allowInput)
            {
                switch (PlayMode)
                {
                    case 0:
                        break;
                    /*
                     * Normaler Spielmodus
                     */
                    case 1:
                        /*
                         * Spielerzug, außerhalb weiterer Animationen
                         */
                        if (Input.GetMouseButton(1))
                            Debug.Log(Turn + "|" + Global.MovingCards);
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
                                    Global.UnhoverCard(Global.prev);
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
                                    Global.UnhoverCard(Global.prev);
                                    Global.prev = null;
                                }
                                if (!FieldSelect)
                                {
                                    StopAllCoroutines();
                                    Board.RefillCards();
                                    selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                                    Global.HoverCard((BoxCollider)hit.collider);
                                    Card selCard = selected;
                                    matches = Board.Platz.FindAll(x => x.Monat == selCard.Monat);
                                    for (int i = 0; i < matches.Count; i++)
                                    {
                                        StartCoroutine(matches[i].BlinkCard());
                                    }
                                }
                                else if (matches.Exists(x => x.Name == hit.collider.gameObject.name))
                                {
                                    selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                                    Global.HoverCard((BoxCollider)hit.collider);
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
                                    Global.UnhoverCard(Global.prev);
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
                        break;
                    /*
                     * Aktionen beim Ziehen einer Karte, Ähnlich normaler Aktionen
                     */
                    case 2:
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
                                            Global.UnhoverCard(Global.prev);
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
                                            Global.UnhoverCard(Global.prev);
                                            Global.prev = null;
                                        }
                                        if (matches.Exists(x => x.Name == hit.collider.gameObject.name))
                                        {
                                            selected = hit.collider.gameObject.GetComponent<CardRef>().card;
                                            Global.HoverCard((BoxCollider)hit.collider);
                                        }
                                    }
                                    else if (hit.collider == null && Global.prev != null)
                                    {
                                        if (Global.prev != null)
                                        {
                                            selected = null;
                                            Global.UnhoverCard(Global.prev);
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
                                            if (!oYaku.Exists(x => x.Key.Name == ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku[i].Key.Name && x.Value == ((Player)(Board.players[Turn ? 0 : 1])).CollectedYaku[i].Value))
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
                        break;
                }
            }
            YakuActions();
            if (Global.Settings.mobile)
                UpdateMobile();
        }
        public void YakuActions()
        {
            if (newYaku.Count > 0)
            {
                if (animLeft != 0 && newYaku.Count != 0)
                {
                    allowInput = false;
                    if (animLeft < 0)
                        animLeft += 10;
                    else if (animLeft != 0)
                        animLeft = 0;
                }
                if (animLeft == 0 && !watch.IsRunning)
                    watch.Start();
                if (watch.ElapsedMilliseconds > 3000)
                {
                    float cWidth = 200;
                    if (newYaku[0].Name.Contains("kou"))
                    {
                        Destroy(GameObject.FindGameObjectWithTag("Yaku"));
                        cWidth /= 2;
                    }
                    else
                        GameObject.FindGameObjectWithTag("Yaku").tag = "oldYaku";
                    newYaku.RemoveAt(0);
                    if (newYaku.Count == 0)
                    {
                        if (_CherryBlossoms)
                            Destroy(_CherryBlossoms);
                        initKoikoi = true;
                        animLeft = 0;
                        return;
                    }
                    watch.Reset();
                    watch.Start();
                    animLeft = -cWidth;
                    shownYaku = false;
                }
                if (newYaku[0].Name.Contains("kou"))
                    KouYaku();
                else
                {
                    if (shownYaku)
                        DrawSlide();
                    else if (newYaku[0].addPoints == 0)
                        FixedYaku();
                    else
                        AddYaku();
                }
            }
            /*
             * Koi Koi Abfrage:
             *  - Animation bei Ja
             *  - Weiterleitung zum Rundenende-Screen bei Nein
             */
            else if (newYaku.Count == 0 && initKoikoi)
                AskKoikoi();
        }

        private void DrawSlide()
        {
            GameObject.Find(GameObject.FindGameObjectWithTag("Yaku").name + "/Yaku").GetComponent<RectTransform>().localPosition = new Vector3(animLeft, 0, 0);
            if (GameObject.FindGameObjectWithTag("oldYaku"))
            {
                float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
                GameObject.Find(GameObject.FindGameObjectWithTag("oldYaku").name + "/Yaku").GetComponent<RectTransform>().localPosition = new Vector3(animLeft + cWidth, 0, 0);
                if (animLeft >= 0)
                    Destroy(GameObject.FindGameObjectWithTag("oldYaku"));
            }
        }

        public void AskKoikoi()
        {
            initKoikoi = false;
            Destroy(GameObject.FindGameObjectWithTag("oldYaku"));
            GameObject Koikoi = Instantiate(Global.prefabCollection.Koikoi);
            Koikoi.name = "Koikoi";
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find("Koikoi/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                initKoikoi = false;
                ((Player)Board.players[Turn ? 0 : 1]).Koikoi++;
                StartCoroutine(Global.KoikoiAnimation(() =>
                {
                    allowInput = true;
                    Turn = !Turn;
                    PlayMode = 1;
                }, Global.prefabCollection.KoikoiText));
                Destroy(Koikoi);
            });
            GameObject.Find("Koikoi/YesButton").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                initKoikoi = false;
                Global.players = Board.players.Cast<Player>().ToList();
                SceneManager.LoadScene("Finish");
            });
            GameObject.Find("Koikoi/NoButton").GetComponent<EventTrigger>().triggers.Add(entry);
        }

        private void AddYaku()
        {
            shownYaku = true;
            GameObject yaku = Instantiate(Global.prefabCollection.gAddYaku);
            yaku.name = newYaku[0].Name;
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find(newYaku[0].Name + "Yaku/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            GameObject.Find(newYaku[0].Name + "Yaku/Title/Shadow").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Name + "Yaku/Title/Text").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Name + "Yaku/Subtitle/Shadow").GetComponent<Text>().text = newYaku[0].Name;
            GameObject.Find(newYaku[0].Name + "Yaku/Subtitle/Text").GetComponent<Text>().text = newYaku[0].Name;
        }

        private void FixedYaku()
        {
            shownYaku = true;
            GameObject yaku = Instantiate(Global.prefabCollection.gFixedYaku);
            yaku.name = newYaku[0].Name;
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find(newYaku[0].Name + "Yaku/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            if (newYaku[0].Name == "Ino Shika Chou")
            {
                GameObject.Find(newYaku[0].Name + "Yaku/Title/Shadow").GetComponent<Text>().font = Global.prefabCollection.EdoFont;
                GameObject.Find(newYaku[0].Name + "Yaku/Title/Text").GetComponent<Text>().font = Global.prefabCollection.EdoFont;
            }
            GameObject.Find(newYaku[0].Name + "Yaku/Title/Shadow").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Name + "Yaku/Title/Text").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Name + "Yaku/Subtitle/Shadow").GetComponent<Text>().text = newYaku[0].Name;
            GameObject.Find(newYaku[0].Name + "Yaku/Subtitle/Text").GetComponent<Text>().text = newYaku[0].Name;
            List<Card> temp = new List<Card>();
            if (newYaku[0].Mask[1] == 1)
                temp.AddRange(((Player)Board.players[0]).CollectedCards.FindAll(x => newYaku[0].Namen.Contains(x.Name)));
            if (newYaku[0].Mask[0] == 1)
                temp.AddRange((((Player)Board.players[0]).CollectedCards.FindAll(x => x.Typ == newYaku[0].TypPref)));
            Transform parent = GameObject.Find(newYaku[0].Name + "Yaku/Cards").transform;
            for (int yakuCard = 0; yakuCard < newYaku[0].minSize; yakuCard++)
            {
                GameObject card = new GameObject(yakuCard.ToString());
                RectTransform rect = card.AddComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.localPosition = new Vector3((newYaku[0].minSize / 2f) * -35 + 35 * yakuCard + 17.5f, 0);
                rect.sizeDelta = new Vector2(25, 40);
                GameObject shadow = new GameObject("Shadow");
                shadow.transform.SetParent(card.transform, false);
                shadow.AddComponent<Image>().color = new Color(0, 0, 0);
                rect = shadow.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(2, -2);
                rect.sizeDelta = new Vector2(25, 40);
                GameObject Image = new GameObject("Image");
                Image.transform.SetParent(card.transform, false);
                Image img = Image.AddComponent<Image>();
                Texture2D tex = (Texture2D)temp[yakuCard].Image.mainTexture;
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                rect = img.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(0, 0);
                rect.sizeDelta = new Vector2(25, 40);
            }
        }

        private void KouYaku()
        {
            if (!shownYaku)
            {
                shownYaku = true;
                GameObject Kou = Instantiate(Global.prefabCollection.gKouYaku);
                Kou.name = newYaku[0].Name;
                Image Text = GameObject.Find(newYaku[0].Name + "/Text").GetComponent<Image>();
                Text.sprite = Resources.Load<Sprite>("Images/" + newYaku[0].Name);
                List<Card> Matches = Global.allCards.FindAll(y => y.Typ == Card.Typen.Lichter);
                for (int cardID = 0; cardID < 5; cardID++)
                {
                    Image card = GameObject.Find(newYaku[0].Name + "/" + cardID.ToString() + "/Card").GetComponent<Image>();
                    if (((Player)Board.players[0]).CollectedCards.Exists(x => x.Name == Matches[cardID].Name))
                    {
                        Texture2D tex = (Texture2D)Matches[cardID].Image.mainTexture;
                        card.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                    }
                    else
                        card.sprite = Global.CardSkins[Global.Settings.CardSkin];
                }
            }
        }

        public void OnGUI()
        {
            GUI.skin = Skin;
            if (GUI.Button(new Rect(0, 0, 20, 20), "X"))
            {
                ((Player)Board.players[0]).CollectedCards = Global.allCards;
            }
            if (Global.Settings.mobile)
                OnGUIMobile();
            else
                OnGUIPC();
        }
    }
}