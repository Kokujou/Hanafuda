using ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = System.Random;
using UnityEngine.Networking;
using Photon.Pun;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        private const int MaxDispersionPos = 5;
        private const int MaxDispersionAngle = 60;
        private const float CardWidth = 11f;

        Transform EffectCam, Hand1, Hand2, Field3D, Deck3D;
        private Communication PlayerInteraction;
        public void Init(List<Player> Players)
        {
            players = Players;
            gameObject.AddComponent<GameUI>();
            PlayerInteraction = Global.instance.GetComponent<Communication>();
            currentAction = new PlayerAction();
            currentAction.Init(this);
            if (Settings.Multiplayer)
            {
                PlayerInteraction.OnDeckSync = GenerateDeck;
                PlayerInteraction.DeckSyncSet = true;
                PlayerInteraction.OnMoveSync = ApplyMove;
                PlayerInteraction.MoveSyncSet = true;
                if (PhotonNetwork.IsMasterClient)
                    PlayerInteraction.BroadcastSeed(UnityEngine.Random.Range(0, 10000));
            }
            else
                GenerateDeck(5);
        }

        private void GenerateDeck(int seed = -1)
        {
            gameObject.AddComponent<PlayerComponent>().Init(players);
            var rnd = seed == -1 ? new Random() : new Random(seed);
            for (var i = 0; i < Global.allCards.Count; i++)
            {
                var rand = rnd.Next(0, Global.allCards.Count);
                while (Deck.Exists(x => x.Title == Global.allCards[rand].Title))
                    rand = rnd.Next(0, Global.allCards.Count);
                Deck.Add(Global.allCards[rand]);
            }
            FieldSetup();
        }

        void Start()
        {
            Camera.main.SetCameraRect();
            Collection = new List<Card>();
            Hovered = new Card[] { };
            Deck = new List<Card>();
            Field = new List<Card>();
            NewYaku = new List<Yaku>();
            currentAction = new PlayerAction();
            _Turn = Settings.PlayerID == 0;
            EffectCam = MainSceneVariables.variableCollection.EffectCamera;
            if (Settings.Mobile)
            {
                Hand1 = MainSceneVariables.variableCollection.Hand1M;
                Hand2 = MainSceneVariables.variableCollection.Hand2M;
                Field3D = MainSceneVariables.variableCollection.MFeld;
                Deck3D = MainSceneVariables.variableCollection.MDeck;
            }
            else
            {
                MainSceneVariables.variableCollection.ExCol.gameObject.SetActive(false);
                Hand1 = MainSceneVariables.variableCollection.Hand1;
                Hand2 = MainSceneVariables.variableCollection.Hand2;
                Field3D = MainSceneVariables.variableCollection.Feld;
                Deck3D = MainSceneVariables.variableCollection.Deck;
            }
            if (Settings.Rounds == 0)
            {
                if (Settings.Multiplayer)
                    Init(Settings.Players);
                else
                {
                    Settings.Players[1 - Settings.PlayerID] = new KI((KI.Mode)Settings.KIMode, "Computer");
                    Init(Settings.Players);
                }
            }
            else
            {
                Init(Settings.Players);
                for (int i = 0; i < players.Count; i++)
                    ((Player)players[i]).Reset();
            }
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
                Field.Add(Deck[0]);
                GameObject temp = Deck[0].Object;
                Deck.RemoveAt(0);
                temp.layer = LayerMask.NameToLayer("Feld");
                temp.transform.parent = Field3D.transform;
                int rows = 2;
                float factor = 1;
                if (Settings.Mobile)
                {
                    rows = 3;
                    factor = 1.5f;
                }
                float offsetX = temp.transform.localScale.x / factor;
                float offsetY = temp.transform.localScale.y / factor;
                float cardWidth = temp.GetComponentInChildren<MeshRenderer>().bounds.size.x / factor;
                float cardHeight = temp.GetComponentInChildren<MeshRenderer>().bounds.size.y / factor;
                float alignY = (cardHeight + offsetY) * ((rows - 1) * 0.5f);
                StartCoroutine(temp.transform.StandardAnimation(Field3D.transform.position +
                    new Vector3((i / rows) * (cardWidth + offsetX), -alignY + (i % rows) * (cardHeight + offsetY), 0),
                    new Vector3(0, 180, 0), Animations.StandardScale / factor, (i + 18) * 0.2f));
                //StartCoroutine(temp.transform.StandardAnimation( GameObject.Find("Feld").transform.position + new Vector3((int)(i/2), 0, 0), new Vector3(0, 180 * (1 - i), 0), temp.transform.localScale, 16 * 0.2f));
            }
            if (!Turn && !Settings.Multiplayer)
                StartCoroutine(Animations.AfterAnimation(OpponentTurn));
        }

        private void BuildHands()
        {
            for (int player = 0; player < players.Count; player++)
            {

                bool active = player == Settings.PlayerID;
                for (int card = 0; card < 8; card++)
                {
                    ((Player)players[player]).Hand.Add(Deck[0]);
                    GameObject temp = Deck[0].Object;
                    Deck.RemoveAt(0);
                    temp.transform.parent = active ? Hand1.transform : Hand2.transform;
                    if (!Settings.Mobile)
                        temp.layer = LayerMask.NameToLayer("P" + (active ? 1 : 2).ToString() + "Hand");
                    /* Zugedeckte Transformation mit anschließender Aufdeckrotation */
                    if (Settings.Mobile)
                    {
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position +
                            new Vector3(UnityEngine.Random.Range(-MaxDispersionPos, MaxDispersionPos),
                            UnityEngine.Random.Range(-MaxDispersionPos, MaxDispersionPos), -card / 10f),
                            new Vector3(0, 0, (active ? 0 : 180) + UnityEngine.Random.Range(-MaxDispersionAngle, MaxDispersionAngle)),
                            temp.transform.localScale, (card + 8 * player) * 0.2f, .3f,
                            () => { temp.transform.position += new Vector3(0, 0, 1); }));
                    }
                    else
                    {
                        StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((card) * CardWidth, 0, -card),
                            Vector3.zero, temp.transform.localScale, (card + 8 * player) * 0.2f));
                        if (temp.transform.parent == Hand1.transform)
                            StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position + new Vector3((card) * CardWidth, 0, -card),
                                new Vector3(0, 180, 0), temp.transform.localScale, 18 * 0.2f));
                    }

                }
            }
            if (Settings.Mobile)
            {
                StartCoroutine(Hand1.transform.StandardAnimation(Hand1.transform.position, new Vector3(0, 180, 0), Hand1.transform.localScale, 4f, AddFunc: () =>
                { StartCoroutine(((Player)players[Settings.PlayerID]).Hand.ResortCards(new CardLayout(true))); }));
                StartCoroutine(((Player)players[1 - Settings.PlayerID]).Hand.ResortCards(new CardLayout(true, 4f)));
            }
        }

        private void BuildDeck()
        {
            for (int i = 0; i < Deck.Count; i++)
            {
                GameObject temp = Instantiate(Global.prefabCollection.PKarte);
                temp.name = Deck[i].Title;
                temp.GetComponentsInChildren<MeshRenderer>()[0].material = Deck[i].Image;
                temp.transform.parent = Deck3D.transform;
                temp.transform.localPosition = new Vector3(0, 0, i * 0.015f);
                Deck[i].Object = temp;
            }
        }
    }
}