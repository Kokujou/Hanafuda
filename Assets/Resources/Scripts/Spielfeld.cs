using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/
/// <summary>
/// veraltet
/// </summary>
namespace Hanafuda
{
    public class Spielfeld
    {
        public List<Card> Deck = new List<Card>();
        public bool isFinal;
        public KI.Move LastMove;
        public List<Card> Platz = new List<Card>();
        public List<object> players = new List<object>();
        public float Value;
        public bool _Turn = true;
        Action<bool> TurnCallback;
        public bool Turn
        {
            get { return _Turn; }
            set { TurnCallback(value); }
        }
        public void PlayCard(Card handCard, List<Card> Matches = null, List<Card> source = null)
        {
            Transform tPlatz = Global.Settings.mobile ? MainSceneVariables.variableCollection.MFeld :
                MainSceneVariables.variableCollection.Feld;
            bool fromHand = true;
            if (Matches == null) Matches = new List<Card>();
            if (source == null) source = ((Player)players[Turn ? 0 : 1]).Hand;
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
            Global.global.StopAllCoroutines();
            RefillCards();
            source.Remove(handCard);
            handCard.Objekt.transform.parent = tPlatz;
            handCard.Objekt.layer = LayerMask.NameToLayer("Feld");
            if (Matches.Count > 0)
                Matches.Add(handCard);
            else
            {
                Platz.Add(handCard);
                Global.global.StartCoroutine(handCard.Objekt.transform.StandardAnimation(tPlatz.position +
                    (Global.Settings.mobile ? new Vector3(((Platz.Count - 1) / 3) * (11f / 1.5f), -9 + (18f / 1.5f) * ((Platz.Count - 1) % 3))
                    : new Vector3(((Platz.Count - 1) / 2) * 11f, -9 + 18 * ((Platz.Count - 1) % 2))),
                    new Vector3(0, 180, 0), handCard.Objekt.transform.localScale / (Global.Settings.mobile ? 1.5f : 1f)));
            }
            for (int i = 0; i < Matches.Count; i++)
            {
                if (Global.Settings.mobile)
                {
                    GameObject match = Matches[i].Objekt;
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(Matches[i]);
                    if (i < Matches.Count - 1)
                        Platz.Remove(Matches[i]);
                    Global.global.StartCoroutine(Matches[i].Objekt.transform.StandardAnimation(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Turn ? 0 : Screen.height)),
                        Vector3.zero, Vector3.zero, 0, AddFunc: () => { GameObject.Destroy(match); }));
                }
                else
                {
                    Transform collection = MainSceneVariables.variableCollection.PCCollections[
                        ("Collection" + (Turn ? "1" : "2") + Matches[i].Typ.ToString()).GetHashCode()];
                    Matches[i].Objekt.transform.parent = collection;
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(Matches[i]);
                    if (i < Matches.Count - 1)
                        Platz.Remove(Matches[i]);
                    Matches[i].Objekt.layer = LayerMask.NameToLayer("Collected");
                    Global.global.StartCoroutine(Matches[i].Objekt.transform.StandardAnimation(Matches[i].Objekt.transform.parent.position +
                        new Vector3(5.5f * ((collection.childCount - 1) % 5), -((collection.childCount - 1) / 5) * 2f, -((collection.childCount - 1) / 5)),
                        new Vector3(0, 180, 0), Matches[i].Objekt.transform.localScale / 2));
                }
            }
            if (Global.Settings.mobile)
            {
                Transform Hand = Turn ? MainSceneVariables.variableCollection.Hand1M : MainSceneVariables.variableCollection.Hand2M;
                if (fromHand)
                    Global.global.StartCoroutine(ResortCardsMobile(((Player)players[Turn ? 0 : 1]).Hand, Hand.position, 8, isHand: true, delay: 1));
                Global.global.StartCoroutine(ResortCardsMobile(Platz, tPlatz.position, 3, rowWise: false, delay: 1));
            }
            else
            {
                Transform Hand = Turn ? MainSceneVariables.variableCollection.Hand1 : MainSceneVariables.variableCollection.Hand2;
                if (fromHand)
                    Global.global.StartCoroutine(ResortCards(((Player)players[Turn ? 0 : 1]).Hand, Hand.transform.position, delay: 1));
                Global.global.StartCoroutine(ResortCards(Platz, tPlatz.position, 2, delay: 1));
            }
        }

        public Spielfeld(List<object> Players, Action<bool> turnCallback, int seed = -1)
        {
            players = Players;
            TurnCallback = turnCallback;
            var rnd = seed == -1 ? new Random() : new Random(seed);
            for (var i = 0; i < Global.allCards.Count; i++)
            {
                var rand = rnd.Next(0, Global.allCards.Count);
                while (Deck.Contains(Global.allCards[rand]))
                    rand = rnd.Next(0, Global.allCards.Count);
                Deck.Add(Global.allCards[rand]);
            }
        }

        public Spielfeld(Spielfeld parent, KI.Move move, bool Turn)
        {
            //WICHTIG! Einsammeln bei Kartenzug!
            Deck.AddRange(parent.Deck);
            Platz.AddRange(parent.Platz);
            Value = 0f;
            players = new List<object>
                {new Player(((Player) parent.players[0]).Name), new Player(((Player) parent.players[1]).Name)};
            ((Player)players[0]).Hand.AddRange(((Player)parent.players[0]).Hand);
            ((Player)players[0]).CollectedCards.AddRange(((Player)parent.players[0]).CollectedCards);
            ((Player)players[0]).Koikoi = ((Player)parent.players[0]).Koikoi;
            ((Player)players[0]).tempPoints = ((Player)parent.players[0]).tempPoints;
            ((Player)players[1]).Hand.AddRange(((Player)parent.players[1]).Hand);
            ((Player)players[1]).CollectedCards.AddRange(((Player)parent.players[1]).CollectedCards);
            ((Player)players[1]).Koikoi = ((Player)parent.players[1]).Koikoi;
            ((Player)players[1]).tempPoints = ((Player)parent.players[1]).tempPoints;
            isFinal = !move.Koikoi;
            var matches = Platz.FindAll(x => move.hCard.Monat == x.Monat);
            switch (matches.Count)
            {
                case 0:
                    Platz.Add(move.hCard);
                    break;
                case 1:
                case 3:
                    for (var i = 0; i < matches.Count; i++)
                        Platz.Remove(matches[i]);
                    matches.Add(move.hCard);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.AddRange(matches);
                    break;
                case 2:
                    Platz.Remove(move.fCard);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(move.fCard);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(move.hCard);
                    break;
            }

            ((Player)players[Turn ? 0 : 1]).Hand.Remove(move.hCard);
            Platz.Add(Deck[0]);
            Deck.RemoveAt(0);
            var Yakus = new List<Yaku>();
            Yakus = Yaku.GetYaku(((Player)players[Turn ? 0 : 1]).CollectedCards.ToList());
            var nPoints = 0;
            for (var i = 0; i < Yakus.Count; i++)
            {
                nPoints += Yakus[i].basePoints;
                if (Yakus[i].addPoints != 0)
                    nPoints += (((Player)players[Turn ? 0 : 1]).CollectedCards.Count(x => x.Typ == Yakus[i].TypPref) -
                                Yakus[i].minSize) * Yakus[i].addPoints;
            }

            if (nPoints > ((Player)players[Turn ? 0 : 1]).tempPoints)
                isFinal = true;
            else isFinal = false;
        }

        public void RefillCards()
        {
            for (var i = 0; i < Platz.Count; i++)
                foreach (Transform side in Platz[i].Objekt.transform)
                {
                    var mat = side.gameObject.GetComponent<MeshRenderer>().material;
                    mat.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, .5f));
                }
        }
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
                Global.global.StartCoroutine(toSort[i].Objekt.transform.StandardAnimation(StartPos +
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
                    Global.global.StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position, new Vector3(0, temp.transform.rotation.eulerAngles.y, hand1 ? 0 : 180), temp.transform.localScale, 0, .3f, () =>
                    {
                        GameObject Card = new GameObject();
                        Card.transform.parent = temp.transform.parent;
                        temp.transform.parent = Card.transform;
                        Card.transform.localPosition = new Vector3(0, hand1 ? -8 : 8);
                        temp.transform.localPosition = new Vector3(0, hand1 ? 8 : -8, 0);
                        List<Transform> hand = new List<Transform>(temp.transform.parent.parent.gameObject.GetComponentsInChildren<Transform>());
                        hand.RemoveAll(x => !x.name.Contains("New"));
                        int id = hand.IndexOf(temp.transform.parent);
                        float max = ((Player)players[Turn ? 0 : 1]).Hand.Count - 1;
                        if (max == 0) max = 0.5f;
                        Global.global.StartCoroutine(temp.transform.parent.StandardAnimation(temp.transform.parent.position + new Vector3(0, 0, -id), temp.transform.parent.eulerAngles + new Vector3(0, 0, -60f + (120f / max) * (max - id)), temp.transform.parent.localScale, .6f, .3f, () =>
                        {
                            GameObject oldParent = temp.transform.parent.gameObject;
                            temp.transform.parent = temp.transform.parent.parent;
                            GameObject.Destroy(oldParent);
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
                        Global.global.StartCoroutine(toSort[i].Objekt.transform.StandardAnimation(StartPos +
                            new Vector3((i % iterations) * (11f / 1.5f), -9 + (i / iterations) * (18f / 1.5f), 0),
                            toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                    }
                }
                else
                {
                    iterations = maxSize;
                    for (int i = 0; i < toSort.Count; i++)
                    {
                        Global.global.StartCoroutine(toSort[i].Objekt.transform.StandardAnimation(StartPos +
                            new Vector3((i / iterations) * (11f / 1.5f), -9 + (i % iterations) * (18f / 1.5f), 0),
                            toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                    }
                }
            }
        }
    }
}