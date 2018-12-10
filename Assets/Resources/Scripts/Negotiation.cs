using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
        private GameObject Kartenziehen, Order, Info1, Info2;
        private string P1Selection = "", P2Selection = "";
        private bool P2;
        private string selected = "";
        private string sTurn = Global.Settings.P1Name;
        private bool Turn = true;

        /// <summary>
        ///     Platzierung der repräsentativen Deck-Karten im Kreis und Initialisierung von Startpositionen
        /// </summary>
        private void Start()
        {
            if (Global.Settings.Multiplayer)
            {
                NetworkServer.RegisterHandler(131, OpponentChoice);
                for (var i = 0; i < Global.Settings.playerClients.Count; i++)
                {
                    Global.Settings.playerClients[i].RegisterHandler(131, OpponentChoice);
                    Global.Settings.playerClients[i].RegisterHandler(132, SyncDeck);
                }

                if (Global.Settings.Name == Global.Settings.P2Name)
                {
                    Turn = false;
                    P2 = true;
                }
            }

            if (!P2)
            {
                var seed = Random.Range(0, 100);
                LoadDeck(seed);
                NetworkServer.SendToAll(132, new Global.Message { message = seed.ToString() });
            }
        }

        private void SyncDeck(NetworkMessage msg)
        {
            LoadDeck(Convert.ToInt32(msg.ReadMessage<Global.Message>().message));
        }

        private void LoadDeck(int seed)
        {
            var tempDeck = new List<Card>();
            Kartenziehen = new GameObject("Kartenziehen");
            var rand = new System.Random(seed);
            var all = new List<Card>(Global.allCards);
            for (var i = 0; i < 12; i++)
            {
                var rnd = rand.Next(0, all.Count);
                tempDeck.Add(all[rnd]);
                all.RemoveAll(x => x.Monat == all[rnd].Monat);
                var go = Instantiate(Global.prefabCollection.PKarte, Kartenziehen.transform);
                go.GetComponentsInChildren<MeshRenderer>()[0].material = tempDeck[i].Image;
                go.name = tempDeck[i].name;
                if (Camera.main.aspect < 1)
                {
                    go.transform.localPosition = new Vector3(0, 0, i * 0.1f);
                    go.transform.localScale = new Vector3(go.transform.localScale.x * 1.5f,
                        go.transform.localScale.y * 1.5f, 1f);
                    go.transform.RotateAround(new Vector3(0, -12, 0), Vector3.forward, -60 + 10 * (11 - i));
                    go.layer = 0;
                }
                else
                {
                    go.transform.Rotate(0, 0, 360f / 12f * i);
                    go.transform.Translate(0, 30, 0);
                }

                tempDeck[tempDeck.Count - 1].Objekt = go;
            }

            Order = Instantiate(Global.prefabCollection.PText);
            Order.name = "Order";
            if (Camera.main.aspect < 1)
            {
                Order.SetActive(false);
                Kartenziehen.transform.Translate(new Vector3(0, -13, 0));
                var Slide = Instantiate(Global.prefabCollection.PSlide);
                Slide.transform.SetParent(Kartenziehen.transform, true);
                var SlideScript = Slide.AddComponent<SlideHand>();
                SlideScript.onComplete = OnSelectItem;
                SlideScript.cParent = tempDeck;
            }
        }

        private void OnSelectItem(Card selected)
        {
            Global.prev = null;
            Turn = !Turn;
            var sel = selected.Objekt;
            sel.transform.parent = null;
            sel.layer = 0;
            //!
            StartCoroutine(sel.transform.StandardAnimation(new Vector3(13 * (P2 ? 1 : -1), 18, 0),
                new Vector3(0, 180, 0), sel.transform.localScale, 0));
            var Info = Instantiate(Global.prefabCollection.PText);
            Info.transform.position = new Vector3(P2 ? 0 : -24, 41, 0);
            Info.GetComponent<TextMesh>().text = "Spieler " + (P2 ? "2" : "1");
            Info.GetComponentsInChildren<TextMesh>()[1].text = "Spieler " + (P2 ? "2" : "1");
            if (P2) Info2 = Info;
            else Info1 = Info;
            if (!Global.Settings.Multiplayer)
            {
                StartCoroutine(AnimOponentChoice(sel, true));
            }
            else
            {
                if (P2)
                {
                    P2Selection = sel.name;
                    Global.Settings.playerClients[0].Send(131, new Global.Message { message = sel.name });
                }
                else
                {
                    P1Selection = sel.name;
                    NetworkServer.SendToAll(131, new Global.Message { message = sel.name });
                }

                if (P1Selection != "" && P2Selection != "")
                    StartCoroutine(GetResult(P1Selection, P2Selection));
            }
        }

        private void OpponentChoice(NetworkMessage msg)
        {
            var card = msg.ReadMessage<Global.Message>().message;
            if (P2) P1Selection = card;
            else P2Selection = card;
            var col = GameObject.Find(card).GetComponent<BoxCollider>();
            col.gameObject.transform.parent = null;
            col.gameObject.layer = 0;
            StartCoroutine(col.gameObject.transform.StandardAnimation(new Vector3(
                    (P2 ? -1 : 1) * (Global.Settings.mobile ? 13 : 55),
                    Global.Settings.mobile ? 18 : 0, 0), new Vector3(0, 180, 0),
                col.gameObject.transform.localScale * (Global.Settings.mobile ? 1 : 2), 0));
            var Info = Instantiate(Global.prefabCollection.PText);
            if (Global.Settings.mobile)
                Info.transform.position = new Vector3(P2 ? -24 : 0, 41, 0);
            else
                Info.transform.position = new Vector3(P2 ? -65 : 40, 30, 0);
            Info.GetComponent<TextMesh>().text = "Spieler " + (P2 ? "1" : "2");
            Info.GetComponentsInChildren<TextMesh>()[1].text = "Spieler " + (P2 ? "1" : "2");
            if (P2) Info1 = Info;
            else Info2 = Info;
            Turn = !Turn;
            if (P1Selection != "" && P2Selection != "")
                StartCoroutine(GetResult(P1Selection, P2Selection));
        }

        private IEnumerator GetResult(string P1Sel, string P2Sel)
        {
            var tempTurn = false;
            if (Global.allCards.Find(x => x.name == P2Sel).Monat < Global.allCards.Find(x => x.name == P1Sel).Monat)
            {
                Info1.GetComponent<TextMesh>().color = new Color(165, 28, 28, 255) / 255;
                Info2.GetComponent<TextMesh>().color = new Color(28, 165, 28, 255) / 255;
                tempTurn = false;
                Global.Turn = 1;
            }
            else
            {
                Info2.GetComponent<TextMesh>().color = new Color(165, 28, 28, 255) / 255;
                Info1.GetComponent<TextMesh>().color = new Color(28, 165, 28, 255) / 255;
                tempTurn = true;
                Global.Turn = 0;
            }

            yield return new WaitForSeconds(3f);
            StopAllCoroutines();
            SceneManager.LoadScene("Singleplayer");
        }

        /// <summary>
        ///     Animation der Wahl des Gegners durch Hervorheben im Urzeigersinn und Laden des nächsten Bildschirms
        /// </summary>
        /// <param name="sel">Wahl des Spielers</param>
        /// <returns></returns>
        private IEnumerator AnimOponentChoice(GameObject sel, bool mobile = false)
        {
            yield return new WaitForSeconds(1);
            float rndTime = Random.Range(1000, 2000);
            var watch = new Stopwatch();
            var cols = new List<BoxCollider>();
            cols.AddRange(Kartenziehen.GetComponentsInChildren<BoxCollider>());
            watch.Start();
            var i = 0;
            while (watch.ElapsedMilliseconds < rndTime)
            {
                if (Global.prev != null)
                {
                    Global.prev.HoverCard(true);
                    Global.prev = null;
                }

                cols[i].HoverCard();
                yield return new WaitForSeconds(.05f);
                i++;
                if (i >= cols.Count)
                    i = 0;
            }

            if (Global.prev != null)
            {
                Global.prev.HoverCard(true);
                Global.prev = null;
            }

            cols[i].gameObject.transform.parent = null;
            StartCoroutine(cols[i].gameObject.transform.StandardAnimation(new Vector3(mobile ? 13 : 55, mobile ? 18 : 0, 0), new Vector3(0, 180, 0),
                cols[i].gameObject.transform.localScale * (mobile ? 1 : 2), 0));
            Order.GetComponentsInChildren<TextMesh>()[0].text = "Spiel wird\ngestartet...";
            Order.GetComponentsInChildren<TextMesh>()[1].text = "Spiel wird\ngestartet...";
            Info2 = Instantiate(Global.prefabCollection.PText);
            if (mobile)
                Info2.transform.position = new Vector3(0, 41, 0);
            else
                Info2.transform.position = new Vector3(40, 30, 0);
            Info2.GetComponent<TextMesh>().text = "Spieler 2";
            Info2.GetComponentsInChildren<TextMesh>()[1].text = "Spieler 2";
            StartCoroutine(GetResult(sel.name, cols[i].name));
            //FieldSetup();
        }

        /// <summary>
        ///     Hover und Auswahl von Karten
        /// </summary>
        private void Update()
        {
            Camera.main.SetCameraRect();
            if (Turn)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Camera.main.aspect > 1)
                {
                    if (Input.GetMouseButtonDown(0) && selected != "")
                    {
                        Turn = !Turn;
                        var sel = GameObject.Find(selected);
                        selected = "";
                        if (Global.prev != null)
                        {
                            Global.prev.HoverCard(true);
                            Global.prev = null;
                        }

                        sel.transform.parent = null;
                        sel.layer = 0;
                        StartCoroutine(sel.transform.StandardAnimation(new Vector3(55 * (P2 ? 1 : -1), 0, 0),
                            new Vector3(0, 180, 0), sel.transform.localScale * 2, 0));
                        var Info = Instantiate(Global.prefabCollection.PText);
                        Info.transform.position = new Vector3(P2 ? 40 : -65, 30, 0);
                        Info.GetComponent<TextMesh>().text = "Spieler " + (P2 ? "2" : "1");
                        Info.GetComponentsInChildren<TextMesh>()[1].text = "Spieler " + (P2 ? "2" : "1");
                        if (P2) Info2 = Info;
                        else Info1 = Info;
                        if (!Global.Settings.Multiplayer)
                        {
                            StartCoroutine(AnimOponentChoice(sel));
                        }
                        else
                        {
                            if (P2)
                            {
                                P2Selection = sel.name;
                                Global.Settings.playerClients[0].Send(131, new Global.Message { message = sel.name });
                            }
                            else
                            {
                                P1Selection = sel.name;
                                NetworkServer.SendToAll(131, new Global.Message { message = sel.name });
                            }

                            if (P1Selection != "" && P2Selection != "")
                                StartCoroutine(GetResult(P1Selection, P2Selection));
                        }
                    }
                    else if (Physics.Raycast(ray, out hit, 5000f, 1 << LayerMask.NameToLayer("Card")) &&
                             selected != hit.collider.gameObject.name)
                    {
                        if (Global.prev != null)
                        {
                            Global.prev.HoverCard(true);
                            Global.prev = null;
                        }

                        selected = hit.collider.gameObject.name;
                        ((BoxCollider)hit.collider).HoverCard();
                    }
                    else if (hit.collider == null)
                    {
                        selected = "";
                        if (Global.prev != null)
                        {
                            Global.prev.HoverCard(true);
                            Global.prev = null;
                        }
                    }
                }
            }
        }
    }
}