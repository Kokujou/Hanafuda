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
            List<Card> activeHand = ((Player)players[Turn ? 0 : 1]).Hand;
            bool fromHand = true;
            if (Matches == null) Matches = new List<Card>();
            if (source == null) source = activeHand;
            else fromHand = false;
            string action = " und sammelt ";
            int j = 0;
            for (j = 0; j < Matches.Count; j++)
            {
                action += Matches[j].Title + ", ";
            }
            if (j == 0) action = " und legt sie aufs Spielfeld.";
            else action = action.Remove(action.Length - 2, 1) + "ein.";
            Global.Spielverlauf.Add(Global.Settings.Name + " wählt " + handCard.Title + action);
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
                    Transform collection = MainSceneVariables.variableCollection.PCCollections[(Turn?0:1)*4 +(int)Matches[i].Typ];
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
                if (fromHand)
                    Global.global.StartCoroutine(activeHand.ResortCards(8, isMobileHand: true, delay: 1));
                Global.global.StartCoroutine(Platz.ResortCards(3, rowWise: false, delay: 1));
            }
            else
            {
                if (fromHand)
                    Global.global.StartCoroutine(activeHand.ResortCards(8, delay: 1));
                Global.global.StartCoroutine(Platz.ResortCards(2,rowWise:false, delay: 1));
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
                while (Deck.Exists(x=>x.Title == Global.allCards[rand].Title))
                    rand = rnd.Next(0, Global.allCards.Count);
                Deck.Add(Global.allCards[rand]);
            }
        }
        /// <summary>
        /// KI-Konstruktor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="move"></param>
        /// <param name="Turn"></param>
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
            {
                foreach (Transform side in Platz[i].Objekt.transform)
                {
                    var mat = side.gameObject.GetComponent<MeshRenderer>().material;
                    mat.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, .5f));
                }
            }
        }
        /// <summary>
        /// Animation des KoiKoi Schriftzugs (Vergrößerung)
        /// </summary>
        /// <param name="append">Aktion bei Vollendung der Animation</param>
        /// <returns></returns>
        public void DrawTurn(int[] move)
        {
            Card hCard = ((Player)players[Turn ? 0 : 1]).Hand[move[0]];
            Card fCard = move[1] >= 0 ? Platz[move[1]] : null;
            List<Card> Matches = Platz.FindAll(x => x.Monat == hCard.Monat);
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
    }
}