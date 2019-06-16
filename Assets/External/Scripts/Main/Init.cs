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
    public partial class Spielfeld : ISpielfeld
    {
        public override void Init(List<Player> Players)
        {
            base.Players = Players;
            PlayerInteraction = Global.instance.GetComponent<Communication>();
            currentAction = new PlayerAction();
            currentAction.Init(this);
            Deck = new List<Card>();
            Field = new List<Card>();
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
                GenerateDeck();
        }

        protected override void GenerateDeck(int seed = -1)
        {
            gameObject.AddComponent<PlayerComponent>().Init(Players);
            var rnd = seed == -1 ? new Random() : new Random(seed);
            List<int> indices = Enumerable.Range(0, Global.allCards.Count).ToList();
            Deck.Clear();
            for (var i = indices.Count - 1; i >= 0; i--)
            {
                var rand = rnd.Next(0, indices.Count);
                Deck.Add(Global.allCards[indices[rand]]);
                indices.RemoveAt(rand);
            }
            FieldSetup();
        }

        void Start()
        {
            Camera.main.SetCameraRect();
            Collection = new List<Card>();
            TurnCollection = new List<Card>();
            Hovered = new Card[] { };
            Deck = new List<Card>();
            Field = new List<Card>();
            currentAction = new PlayerAction();
            Turn = Settings.PlayerID == 0;
            EffectCam = MainSceneVariables.boardTransforms.EffectCamera;
            if (Settings.Mobile)
            {
                Hand1 = MainSceneVariables.boardTransforms.Hand1M;
                Hand2 = MainSceneVariables.boardTransforms.Hand2M;
                Field3D = MainSceneVariables.boardTransforms.MFeld;
                Deck3D = MainSceneVariables.boardTransforms.MDeck;
                InfoUI = Instantiate(Global.prefabCollection.GameInfoMobile).GetComponent<GameInfo>();
            }
            else
            {
                Hand1 = MainSceneVariables.boardTransforms.Hand1;
                Hand2 = MainSceneVariables.boardTransforms.Hand2;
                Field3D = MainSceneVariables.boardTransforms.Feld;
                Deck3D = MainSceneVariables.boardTransforms.Deck;
                InfoUI = Instantiate(Global.prefabCollection.GameInfoPC).GetComponentInChildren<GameInfo>();
            }

            if (!Settings.Consulting)
            {
                if (Settings.Rounds == 0)
                {
                    if (Settings.Multiplayer)
                        Init(Settings.Players);
                    else
                    {
                        Settings.Players[1 - Settings.PlayerID] = KI.Init((Settings.AIMode)Settings.AiMode, "Computer");
                        Init(Settings.Players);
                    }
                }
                else
                {
                    for (int i = 0; i < Settings.Players.Count; i++)
                        (Settings.Players[i]).Reset();
                    Init(Settings.Players);
                }
                InfoUI.GetYakuList(0).BuildFromCards(new List<Card>(), Players[0].CollectedYaku);
                InfoUI.GetYakuList(1).BuildFromCards(new List<Card>(), Players[1].CollectedYaku);
            }
            else Settings.Players[1 - Settings.PlayerID] = KI.Init((Settings.AIMode)Settings.AiMode, "Computer");
        }

        /// <summary>
        /// Erstellung des Decks, sowie Austeilen von Händen und Spielfeld
        /// </summary>
        protected override void FieldSetup()
        {
            BuildDeck();
            BuildHands();
            BuildField();
        }
        public override void BuildField(int fieldSize = 8)
        {
            for (int i = 0; i < fieldSize; i++)
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
            foreach (Player player in Players)
                if (player.Hand.IsInitialWin() != 0) DrawnGame();
            if (!Turn && !Settings.Multiplayer)
                StartCoroutine(Animations.AfterAnimation(OpponentTurn));
        }

        public override void BuildHands(int hand1Size = 8, int hand2Size = 8)
        {
            int[] handSizes = new int[] { hand1Size, hand2Size };
            for (int player = 0; player < Players.Count; player++)
            {
                bool active = player == Settings.PlayerID;
                for (int card = 0; card < handSizes[player]; card++)
                {
                    ((Player)Players[player]).Hand.Add(Deck[0]);
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
                { StartCoroutine(((Player)Players[Settings.PlayerID]).Hand.ResortCards(new CardLayout(true))); }));
#if UNITY_EDITOR
                StartCoroutine(Hand2.transform.StandardAnimation(Hand2.transform.position, new Vector3(0, 180, 0), Hand2.transform.localScale, 4f, AddFunc: () =>
                { StartCoroutine(((Player)Players[1 - Settings.PlayerID]).Hand.ResortCards(new CardLayout(true))); }));
#else
                StartCoroutine(((Player)players[1 - Settings.PlayerID]).Hand.ResortCards(new CardLayout(true, 4f)));
#endif
            }
        }

        public override void BuildDeck()
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