using ExtensionMethods;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Photon.Pun;

/*
 * Todo:
*/
/// <summary>
/// Klasse zur Regelung der Oya-Aushandlung
/// </summary>
/// 
namespace Hanafuda
{
    public class Negotiation : MonoBehaviour
    {
        private GameObject Kartenziehen, Order;
        private GameObject[] Infos;
        private Card[] Hovered;
        private Communication PlayerInteraction;
        public Card[] Selections;
        public List<Card> tempDeck;
        private int _Turn = -1;
        public int Turn
        {
            get { return _Turn; }
            set
            {
                _Turn = value;
                if (Settings.Mobile && value == Settings.PlayerID)
                    StartCoroutine(Animations.AfterAnimation(CreateSlide));
            }
        }
        private Card selected;
        /// <summary>
        ///     Platzierung der repräsentativen Deck-Karten im Kreis und Initialisierung von Startpositionen
        /// </summary>
        public void Start()
        {
            Selections = new Card[Settings.Players.Count];
            Infos = new GameObject[Settings.Players.Count];
            Hovered = new Card[] { };
            PlayerInteraction = Global.instance.gameObject.GetComponent<Communication>();
            var seed = Random.Range(0, 100);
            if (Settings.Multiplayer)
            {
                PlayerInteraction.OnDeckSync = LoadDeck;
                PlayerInteraction.DeckSyncSet = true;
                PlayerInteraction.OnMoveSync = PlayCard;
                PlayerInteraction.MoveSyncSet = true;
                if (PhotonNetwork.IsMasterClient)
                    PlayerInteraction.BroadcastSeed(seed);
            }
            else
            {
                Destroy(PlayerInteraction);
                LoadDeck(seed);
            }
        }

        private void LoadDeck(int seed)
        {
            tempDeck = new List<Card>();
            Kartenziehen = new GameObject("Kartenziehen");
            var rand = new System.Random(seed);
            var all = new List<Card>(Global.allCards);
            for (var i = 0; i < 12; i++)
            {
                var rnd = rand.Next(0, all.Count);
                tempDeck.Add(all[rnd]);
                Card.Months month = all[rnd].Monat;
                all.RemoveAll(x => x.Monat == month);
                var go = Instantiate(Global.prefabCollection.PKarte, Kartenziehen.transform);
                go.GetComponentsInChildren<MeshRenderer>()[0].material = tempDeck[i].Image;
                go.name = tempDeck[i].Title;
                if (Settings.Mobile)
                {
                    go.transform.localPosition = new Vector3(0, 0, i * 0.1f);
                    go.transform.localScale = Animations.StandardScale * 1.5f;
                    go.transform.RotateAround(new Vector3(0, -12, 0), Vector3.forward, -60 + 10 * (11 - i));
                    go.layer = 0;
                }
                else
                {
                    go.transform.Rotate(0, 0, 360f / 12f * i);
                    go.transform.Translate(0, 30, 0);
                }
                tempDeck[i].Object = go;
            }
            Order = Instantiate(Global.prefabCollection.PText);
            Order.name = "Order";
            if (Settings.Mobile)
            {
                Order.SetActive(false);
                Kartenziehen.transform.Translate(new Vector3(0, -13, 0));
            }
            Turn = 0;
        }
        public void HoverCards(params Card[] cards)
        {
            for (int card = 0; card < Hovered.Length; card++)
                Hovered[card]?.HoverCard(true);
            for (int card = 0; card < cards.Length; card++)
                cards[card]?.HoverCard();
            Hovered = cards;
        }
        public void OnSelectItem(Card Selection)
        {
            PlayerAction action = new PlayerAction();
            action.SingleSelection = Selection;
            action.PlayerID = Settings.PlayerID;
            if (Settings.Multiplayer)
                PlayerInteraction.SendAction(action);
            else
                PlayCard(action);
            //PlayCardMobile(action);
        }

        /// <summary>
        ///     Animation der Wahl des Gegners durch Hervorheben im Urzeigersinn und Laden des nächsten Bildschirms
        /// </summary>
        /// <param name="sel">Wahl des Spielers</param>
        /// <returns></returns>
        public IEnumerator AnimOpponentChoice(Card.Months outcome = Card.Months.Null)
        {
            yield return new WaitForSeconds(1);
            float rndTime = Random.Range(1000, 2000);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var i = 0;
            if (!Settings.Mobile)
            {
                TextMesh[] captions = Order.GetComponentsInChildren<TextMesh>();
                for (int caption = 0; caption < captions.Length; caption++)
                    captions[caption].text = "Gegner wählt\naus";
            }
            while (watch.ElapsedMilliseconds < rndTime || (outcome != Card.Months.Null && tempDeck[i].Monat != outcome))
            {
                BoxCollider col = tempDeck[i].Object.GetComponent<BoxCollider>();
                Global.prev?.HoverCard(true);
                col.HoverCard();
                yield return new WaitForSeconds(.05f);
                i++;
                if (i >= tempDeck.Count)
                    i = 0;
            }
            Global.prev?.HoverCard(true);
            PlayerAction action = new PlayerAction();
            action.SingleSelection = tempDeck[i];
            action.PlayerID = 1;
            PlayCard(action);
            if (!Settings.Mobile)
            {
                TextMesh[] captions = Order.GetComponentsInChildren<TextMesh>();
                for (int caption = 0; caption < captions.Length; caption++)
                    captions[caption].text = "Frühster Monat\ngewinnt";
            }
        }

        public void PlayCard(Move action)
        {
            bool isHost = action.PlayerID == 1;
            float targetX = 13, targetY = 18, targetScale = 1.5f, InfoY = 35;
            if (!Settings.Mobile)
            {
                targetX = 55;
                targetY = 0;
                targetScale = 2;
                InfoY = 25;
            }
            Card SingleSelection = tempDeck.Find(x => x.Title == action.SingleSelection);
            Debug.Log($"Player{ action.PlayerID} zog {SingleSelection.Monat}");
            Selections[action.PlayerID] = SingleSelection;
            Global.prev = null;
            GameObject sel = SingleSelection.Object;
            sel.transform.parent = null;
            tempDeck.Remove(SingleSelection);
            sel.layer = 0;
            StartCoroutine(sel.transform.StandardAnimation(new Vector3(targetX * (isHost ? 1 : -1), targetY, 0),
                new Vector3(0, 180, 0), Animations.StandardScale * targetScale, 0));
            var Info = Instantiate(Global.prefabCollection.PText);
            Info.transform.position = new Vector3(targetX * (isHost ? 1 : -1), InfoY, 0);
            Info.GetComponent<TextMesh>().text = Settings.Players[action.PlayerID].Name;
            Info.GetComponentsInChildren<TextMesh>()[1].text = Settings.Players[action.PlayerID].Name;
            Infos[action.PlayerID] = Info;
            Turn = action.PlayerID + 1;
            for (int selection = 0; selection < Selections.Length; selection++)
                if (Selections[selection] == null)
                {
                    if (!Settings.Multiplayer && GetComponent<Tutorial>() == null)
                        StartCoroutine(AnimOpponentChoice());
                    return;
                }
            StartCoroutine(GetResult());
        }

        private IEnumerator GetResult()
        {
            SortedList<int, int> selections = new SortedList<int, int>();
            for (int selection = 0; selection < Selections.Length; selection++)
                selections.Add((int)Selections[selection].Monat, selection);
            List<Player> rearrange = new List<Player>(Settings.Players);
            Infos[selections.Values[0]].GetComponent<TextMesh>().color = new Color(28, 165, 28, 255) / 255f;
            Player self = Settings.Players[Settings.PlayerID];
            Settings.Players[selections.Values[0]] = rearrange[0];
            for (int selection = 1; selection < Selections.Length; selection++)
            {
                Infos[selections.Values[selection]].GetComponent<TextMesh>().color = new Color(165, 28, 28, 255) / 255f;
                Settings.Players[selections.Values[selection]] = rearrange[selection];

            }
            Settings.PlayerID = Settings.Players.IndexOf(self);
            yield return new WaitForSeconds(3f);
            StopAllCoroutines();
            SceneManager.LoadScene("Singleplayer");
        }


        /// <summary>
        ///     Hover und Auswahl von Karten
        /// </summary>
        private void Update()
        {
            Camera.main.SetCameraRect();
            if (Settings.Mobile) return;
            if (Turn == Settings.PlayerID)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Input.GetMouseButtonDown(0) && selected?.Title.Length > 0)
                {
                    PlayerAction action = new PlayerAction();
                    action.SingleSelection = selected;
                    action.PlayerID = Settings.PlayerID;
                    if (Settings.Multiplayer)
                        PlayerInteraction.SendAction(action);
                    else
                        PlayCard(action);
                }
                else if (Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("Card")) &&
                         selected?.Title != hit.collider.gameObject.GetComponent<CardComponent>().card.Title)
                {
                    Global.prev?.HoverCard(true);
                    selected = hit.collider.gameObject.GetComponent<CardComponent>().card;
                    ((BoxCollider)hit.collider)?.HoverCard();
                }
                else if (hit.collider == null)
                {
                    selected = null;
                    Global.prev?.HoverCard(true);
                }
            }
        }

        private void CreateSlide()
        {
            if (Settings.Mobile)
            {
                var Slide = Instantiate(Global.prefabCollection.PSlide);
                Slide.transform.SetParent(Kartenziehen.transform, true);
                var SlideScript = Slide.AddComponent<SlideHand>();
                SlideScript.Init(tempDeck.Count, x => HoverCards(x >= 0 ? tempDeck[x] : null), x => { OnSelectItem(tempDeck[x]); });
            }
        }
    }
}