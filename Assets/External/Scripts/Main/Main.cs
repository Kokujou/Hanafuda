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
 * - Networking in eigene Klasse abdocken
 * - Verlagern der Basisklasse aufs Spielfeld
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
        
        

        public GUISkin Skin;
        public bool allowInput = true;
        internal GameObject Slide, _CherryBlossoms;
        
        internal Spielfeld Board;
        KI Opponent;
        List<Card> matches = new List<Card>();
        Card tHandCard;
        internal List<Yaku> newYaku = new List<Yaku>();
        float time;
        bool FieldSelect;
        /// <summary>
        /// 1: Normal, 2: Kartenzug
        /// </summary>
        internal int PlayMode;
        /// <summary>
        /// 0: Gespielte Karte, 1: Gewählte Feldkarte (falls nötig), 2: zusätzliche gewählte Feldkarte bei Ziehen
        /// </summary>
        int[] Move = new int[] { -1, -1, -1 };

        // Update is called once per frame
        public Card selected;
        void Update()
        {
            Camera.main.SetCameraRect();
            //Global.SetCameraRect(EffectCam.GetComponent<Camera>());
            /*
             * Ablaufplan:
             * Check Hover
             *      Hover Matches
             * Execute Selection
             *      (opt) wait for second selection
             * Draw Card
             *      (opt) wait for second selection
             * Check for new yaku
             *      Draw Yaku + Ask Koi Koi
             * Change Turn
             * Handle Opponents Turn
             */
            if (Global.MovingCards == 0)
            {
                if (allowInput)
                    ExecutePlaymode();
                YakuActions();
                //if (Global.Settings.mobile)
                    //UpdateMobile();
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
                        NetworkServer.SendToAll(MoveSyncMsg, new Message() { message = move });
                    else
                        Global.Settings.playerClients[0].Send(MoveSyncMsg, new Message() { message = move });
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
        public void TurnCallback(bool value)
        {
            if (PlayMode == 1 && Board._Turn)
            {
                // Einsammeln bei Kartenzug? //
                matches = new List<Card>();
                //if (Board.Field.Exists(x => x.Monat == Board.Deck[0].Monat))
                    //matches.AddRange(Board.Field.FindAll(x => x.Monat == Board.Deck[0].Monat));
                selected = Board.Deck[0];
                tHandCard = Board.Deck[0];
                if (matches.Count == 2)
                {
                    if (Global.Settings.mobile)
                    {
                        for (int i = 0; i < Board.Field.Count; i++)
                        {
                            Color col = matches.Exists(x => x.Title == Board.Field[i].Title) ? new Color(.3f, .3f, .3f) : new Color(.5f, .5f, .5f);
                            Board.Field[i].Objekt.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Foreground").material.SetColor("_TintColor", col);
                        }
                    }
                    //else
                        //for (int i = 0; i < 2; i++)
                            //StartCoroutine(matches[i].BlinkCard());
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
            //if (Global.Settings.mobile)
              //  OnGUIMobile();
            //else
              //  OnGUIPC();
        }
    }
}