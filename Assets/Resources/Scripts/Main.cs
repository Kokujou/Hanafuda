using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExtensionMethods;


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
 * - GUI durch GUILayout ersetzen
 */

namespace Hanafuda
{
    /// <summary>
    /// Klasse für die Spielmechaniken im Einzelspielermodus
    /// </summary>
    public partial class Main : NetworkBehaviour
    {
        private const int MoveSyncMsg = 131;
        private const int DeckSyncMsg = 132;
        private const int MaxDispersionPos = 5;
        private const int MaxDispersionAngle = 60;
        private const float CardWidth = 11f;
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
        bool FieldSelect, initKoikoi, shownYaku;
        /// <summary>
        /// 1: Normal, 2: Kartenzug
        /// </summary>
        internal int PlayMode;
        /// <summary>
        /// 0: Gespielte Karte, 1: Gewählte Feldkarte (falls nötig), 2: zusätzliche gewählte Feldkarte bei Ziehen
        /// </summary>
        int[] Move = new int[] { -1, -1, -1 };
        /// <summary>
        /// Vereinbarung von Aktionen beim Rundenwechsel
        /// </summary>
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
                Board = new Spielfeld(new List<object>() { new Player(Global.Settings.P1Name), new Player(Global.Settings.P2Name) }, TurnCallback, seed);
            else
            {
                Board = new Spielfeld(Global.players.Cast<object>().ToList(), TurnCallback, seed);
                for (int i = 0; i < Board.players.Count; i++)
                    ((Player)Board.players[0]).Reset();
            }
            Board._Turn = Global.Settings.Name == ((Player)Board.players[Global.Turn]).Name;
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
                NetworkServer.RegisterHandler(MoveSyncMsg, SyncMove);
                for (int i = 0; i < Global.Settings.playerClients.Count; i++)
                {
                    Global.Settings.playerClients[i].RegisterHandler(MoveSyncMsg, SyncMove);
                    Global.Settings.playerClients[i].RegisterHandler(DeckSyncMsg, SyncDeck);
                }
                if (NetworkServer.active)
                {
                    int seed = UnityEngine.Random.Range(0, 1000);
                    NetworkServer.SendToAll(DeckSyncMsg, new Global.Message() { message = seed.ToString() });
                    GenerateDeck(seed);
                }
                return;
            }
            if (Global.players.Count == 0)
            {
                Board = new Spielfeld(new List<object>() { new Player(Global.Settings.P1Name) }, x => TurnCallback(x));
                Board.players.Add(new KI((KI.Mode)Global.Settings.KIMode, Board, Board.Turn, "Computer"));
            }
            else
            {
                Board = new Spielfeld(Global.players.Cast<object>().ToList(), TurnCallback);
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
                Board.Deck[i].Objekt = temp;
            }
            for (int i = Global.Settings.Name == Global.Settings.P1Name ? 0 : 1;
                Global.Settings.Name == Global.Settings.P1Name ? i < 2 : i >= 0;
                i += (Global.Settings.Name == Global.Settings.P1Name ? 1 : -1))
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
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position +
                            new Vector3(UnityEngine.Random.Range(-MaxDispersionPos, MaxDispersionPos),
                            UnityEngine.Random.Range(-MaxDispersionPos, MaxDispersionPos), -j),
                            new Vector3(0, 0, (i == 0 ? 0 : 180) + UnityEngine.Random.Range(-MaxDispersionAngle, MaxDispersionAngle)),
                            temp.transform.localScale, (j + 8 * i) * 0.2f, .3f,
                            () => { temp.transform.position += new Vector3(0, 0, 1); }));
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position,
                            new Vector3(0, 0, (i == 0 ? 0 : 180)), temp.transform.localScale, 18 * 0.2f, .3f, () =>
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
                           StartCoroutine(temp.transform.parent.StandardAnimation(temp.transform.parent.position + new Vector3(0, 0, id),
                               temp.transform.parent.eulerAngles + new Vector3(0, 0, -60f + (120f / 7) * id),
                               temp.transform.parent.localScale, .6f, .3f, () =>
                                 {
                                     GameObject oldParent = temp.transform.parent.gameObject;
                                     temp.transform.parent = temp.transform.parent.parent;
                                     Destroy(oldParent);
                                     //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, id / 10f);
                                 }));
                       }));
                        StartCoroutine(Hand1.transform.StandardAnimation(Hand1.transform.position, new Vector3(0, 180, 0), Hand1.transform.localScale, 5f));
                    }
                    else
                    {
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((j) * CardWidth, 0, -j),
                            Vector3.zero, temp.transform.localScale, (j + 8 * i) * 0.2f));
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((j) * CardWidth, 0, -j),
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
                    StartCoroutine(temp.transform.StandardAnimation(Platz.transform.position + new Vector3(((i) / 3) * (CardWidth / 1.5f), -9 + (18f / 1.5f) * (i % 3), -i), new Vector3(0, 180, 0), temp.transform.localScale / 1.5f, (i + 18) * 0.2f));
                else
                    StartCoroutine(temp.transform.StandardAnimation(Platz.transform.position + new Vector3((i / 2) * CardWidth, -9 + 18 * (((i) + 1) % 2), -i), new Vector3(0, 180, 0), temp.transform.localScale, (i + 18) * 0.2f));
                //StartCoroutine(temp.transform.StandardAnimation( GameObject.Find("Feld").transform.position + new Vector3((int)(i/2), 0, 0), new Vector3(0, 180 * (1 - i), 0), temp.transform.localScale, 16 * 0.2f));
            }
        }

        // Update is called once per frame
        public Card selected;
        /// <summary>
        /// Animation des KoiKoi Schriftzugs (Vergrößerung)
        /// </summary>
        /// <param name="append">Aktion bei Vollendung der Animation</param>
        /// <returns></returns>
        public void DrawTurn(int[] move)
        {
            Card hCard = ((Player)Board.players[Board.Turn ? 0 : 1]).Hand[move[0]];
            Card fCard = move[1] >= 0 ? Board.Platz[move[1]] : null;
            List<Card> Matches = Board.Platz.FindAll(x => x.Monat == hCard.Monat);
            switch (Matches.Count)
            {
                case 0:
                    Board.PlayCard(hCard);
                    break;
                case 1:
                case 3:
                    Board.PlayCard(hCard, Matches);
                    break;
                case 2:
                    Board.PlayCard(hCard, new List<Card>() { fCard });
                    break;
            }
            Board.Turn = !Board.Turn;
        }

        void Update()
        {
            Camera.main.SetCameraRect();
            //Global.SetCameraRect(EffectCam.GetComponent<Camera>());
            if (allowInput)
                ExecutePlaymode();
            if (Global.MovingCards == 0)
                YakuActions();
            if (Global.Settings.mobile)
                UpdateMobile();
        }
        private void CheckNewYaku()
        {
            int oPoints = ((Player)(Board.players[Board.Turn ? 0 : 1])).tempPoints;
            List<KeyValuePair<Yaku, int>> oYaku = ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku;
            ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku = new List<KeyValuePair<Yaku, int>>(Yaku.GetYaku(((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedCards).ToDictionary(x => x, x => 0));
            newYaku.Clear();
            if (((Player)(Board.players[Board.Turn ? 0 : 1])).tempPoints > oPoints)
            {
                for (int i = 0; i < ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku.Count; i++)
                {
                    if (!oYaku.Exists(x => x.Key.Name == ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku[i].Key.Name && x.Value == ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku[i].Value))
                    {
                        newYaku.Add(((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku[i].Key);
                    }
                }
                _CherryBlossoms = Instantiate(Global.prefabCollection.CherryBlossoms);
            }
            if (newYaku.Count == 0)
            {
                Board._Turn = !Board._Turn;
                PlayMode = 1;
                if (Global.Settings.Multiplayer)
                {
                    string move = Move[0].ToString() + "," + Move[1].ToString() + "," + Move[2].ToString();
                    if (NetworkServer.active)
                        NetworkServer.SendToAll(MoveSyncMsg, new Global.Message() { message = move });
                    else
                        Global.Settings.playerClients[0].Send(MoveSyncMsg, new Global.Message() { message = move });
                }
            }
            Move = new[] { -1, -1, -1 };
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
                ((Player)Board.players[Board.Turn ? 0 : 1]).Koikoi++;
                StartCoroutine(Global.prefabCollection.KoikoiText.KoikoiAnimation(() =>
                {
                    allowInput = true;
                    Board.Turn = !Board.Turn;
                    PlayMode = 1;
                }));
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
        public void TurnCallback(bool value)
        {
            if (PlayMode == 1 && Board._Turn)
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
            else Board._Turn = value;
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