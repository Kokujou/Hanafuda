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
 * - !IMPORTANT PC-HAND FIXEN!
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
 * - IMGUI auswechseln
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
                RegisterHandlers();
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
            BuildDeck();
            BuildHands();
            BuildField();
        }

        private void BuildField()
        {
            for (int i = 0; i < 8; i++)
            {
                Board.Platz.Add(Board.Deck[0]);
                GameObject temp = Board.Deck[0].Objekt;
                Board.Deck.RemoveAt(0);
                temp.layer = LayerMask.NameToLayer("Feld");
                temp.transform.parent = Platz.transform;
                int rows = 2;
                float factor = 1;
                if (Global.Settings.mobile)
                {
                    rows = 3;
                    factor = 1.5f;
                }
                float offsetX = temp.transform.localScale.x / factor;
                float offsetY = temp.transform.localScale.y / factor;
                float cardWidth = temp.GetComponentInChildren<MeshRenderer>().bounds.size.x / factor;
                float cardHeight = temp.GetComponentInChildren<MeshRenderer>().bounds.size.y / factor;
                float alignY = (cardHeight + offsetY) * ((rows - 1) * 0.5f);
                StartCoroutine(temp.transform.StandardAnimation(Platz.transform.position + 
                    new Vector3((i / rows) * (cardWidth + offsetX), -alignY + (i % rows) * (cardHeight + offsetY), 0), 
                    new Vector3(0, 180, 0), temp.transform.localScale / factor, (i + 18) * 0.2f));
                //StartCoroutine(temp.transform.StandardAnimation( GameObject.Find("Feld").transform.position + new Vector3((int)(i/2), 0, 0), new Vector3(0, 180 * (1 - i), 0), temp.transform.localScale, 16 * 0.2f));
            }
        }

        private void BuildHands()
        {
            for (int i = Global.Settings.Name == Global.Settings.P1Name ? 0 : 1;
                Global.Settings.Name == Global.Settings.P1Name ? i < 2 : i >= 0;
                i += (Global.Settings.Name == Global.Settings.P1Name ? 1 : -1))
            {
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
                            UnityEngine.Random.Range(-MaxDispersionPos, MaxDispersionPos), -j / 10f),
                            new Vector3(0, 0, (i == 0 ? 0 : 180) + UnityEngine.Random.Range(-MaxDispersionAngle, MaxDispersionAngle)),
                            temp.transform.localScale, (j + 8 * i) * 0.2f, .3f,
                            () => { temp.transform.position += new Vector3(0, 0, 1); }));
                    }
                    else
                    {
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((j) * CardWidth, 0, -j),
                            Vector3.zero, temp.transform.localScale, (j + 8 * i) * 0.2f));
                        if (temp.transform.parent == Hand1.transform)
                            StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((j) * CardWidth, 0, -j),
                                new Vector3(0, 180, 0), temp.transform.localScale, 18 * 0.2f));
                    }

                }
            }
            if (Global.Settings.mobile)
            {
                StartCoroutine(Hand1.transform.StandardAnimation(Hand1.transform.position, new Vector3(0, 180, 0), Hand1.transform.localScale, 4f, AddFunc: () =>
                { StartCoroutine(((Player)Board.players[0]).Hand.ResortCards(8, true)); }));
                StartCoroutine(((Player)Board.players[1]).Hand.ResortCards(8, true, delay: 4f));
            }
        }

        private void BuildDeck()
        {
            for (int i = 0; i < Board.Deck.Count; i++)
            {
                GameObject temp = Instantiate(Global.prefabCollection.PKarte);
                temp.name = Board.Deck[i].Title;
                temp.GetComponentsInChildren<MeshRenderer>()[0].material = Board.Deck[i].Image;
                temp.transform.parent = Deck.transform;
                temp.transform.localPosition = new Vector3(0, 0, i * 0.015f);
                Board.Deck[i].Objekt = temp;
            }
        }

        // Update is called once per frame
        public Card selected;
        void Update()
        {
            Camera.main.SetCameraRect();
            //Global.SetCameraRect(EffectCam.GetComponent<Camera>());
            if (Global.MovingCards == 0)
            {
                if (allowInput)
                    ExecutePlaymode();
                YakuActions();
                if (Global.Settings.mobile)
                    UpdateMobile();
            }
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
                    if (!oYaku.Exists(x => x.Key.Title == ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku[i].Key.Title && x.Value == ((Player)(Board.players[Board.Turn ? 0 : 1])).CollectedYaku[i].Value))
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
        YakuManager yakuManager;
        public void YakuActions()
        {
            if (newYaku.Count > 0 && allowInput)
            {
                yakuManager = Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>();
                yakuManager.Init(newYaku, Board);
                allowInput = false;
            }
            else if (newYaku.Count == 0 && _CherryBlossoms && yakuManager.Finished)
            {
                Destroy(yakuManager.gameObject);
                allowInput = true;
                Destroy(_CherryBlossoms);
            }
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

        /*public void AskKoikoi()
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
        }*/

        private void AddYaku()
        {
            shownYaku = true;
            GameObject yaku = Instantiate(Global.prefabCollection.gAddYaku);
            yaku.name = newYaku[0].Title;
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find(newYaku[0].Title + "Yaku/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            GameObject.Find(newYaku[0].Title + "Yaku/Title/Shadow").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Title + "Yaku/Title/Text").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Title + "Yaku/Subtitle/Shadow").GetComponent<Text>().text = newYaku[0].Title;
            GameObject.Find(newYaku[0].Title + "Yaku/Subtitle/Text").GetComponent<Text>().text = newYaku[0].Title;
        }

        private void FixedYaku()
        {
            shownYaku = true;
            GameObject yaku = Instantiate(Global.prefabCollection.gFixedYaku);
            yaku.name = newYaku[0].Title;
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find(newYaku[0].Title + "Yaku/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            if (newYaku[0].Title == "Ino Shika Chou")
            {
                GameObject.Find(newYaku[0].Title + "Yaku/Title/Shadow").GetComponent<Text>().font = Global.prefabCollection.EdoFont;
                GameObject.Find(newYaku[0].Title + "Yaku/Title/Text").GetComponent<Text>().font = Global.prefabCollection.EdoFont;
            }
            GameObject.Find(newYaku[0].Title + "Yaku/Title/Shadow").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Title + "Yaku/Title/Text").GetComponent<Text>().text = newYaku[0].JName;
            GameObject.Find(newYaku[0].Title + "Yaku/Subtitle/Shadow").GetComponent<Text>().text = newYaku[0].Title;
            GameObject.Find(newYaku[0].Title + "Yaku/Subtitle/Text").GetComponent<Text>().text = newYaku[0].Title;
            List<Card> temp = new List<Card>();
            if (newYaku[0].Mask[1] == 1)
                temp.AddRange(((Player)Board.players[0]).CollectedCards.FindAll(x => newYaku[0].Namen.Contains(x.Title)));
            if (newYaku[0].Mask[0] == 1)
                temp.AddRange((((Player)Board.players[0]).CollectedCards.FindAll(x => x.Typ == newYaku[0].TypPref)));
            Transform parent = GameObject.Find(newYaku[0].Title + "Yaku/Cards").transform;
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
                Kou.name = newYaku[0].Title;
                Image Text = GameObject.Find(newYaku[0].Title + "/Text").GetComponent<Image>();
                Text.sprite = Resources.Load<Sprite>("Images/" + newYaku[0].Title);
                List<Card> Matches = Global.allCards.FindAll(y => y.Typ == Card.Typen.Lichter);
                for (int cardID = 0; cardID < 5; cardID++)
                {
                    Image card = GameObject.Find(newYaku[0].Title + "/" + cardID.ToString() + "/Card").GetComponent<Image>();
                    if (((Player)Board.players[0]).CollectedCards.Exists(x => x.Title == Matches[cardID].Title))
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
                            Color col = matches.Exists(x => x.Title == Board.Platz[i].Title) ? new Color(.3f, .3f, .3f) : new Color(.5f, .5f, .5f);
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
            if (GUI.Button(new Rect(0, 0, 30, 30), "X"))
                ((Player)Board.players[0]).CollectedCards = Global.allCards;
            if (Global.Settings.mobile)
                OnGUIMobile();
            else
                OnGUIPC();
        }
    }
}