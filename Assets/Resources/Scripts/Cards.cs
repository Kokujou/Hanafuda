using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
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
    public class Player
    {
        private List<KeyValuePair<Yaku, int>> _CollectedYaku;
        public List<Card> CollectedCards;
        public List<Card> Hand;
        public int Koikoi;
        public string Name;
        public List<int> pTotalPoints = new List<int>();
        public int tempPoints;
        public int TotalPoints;

        public Player(string name)
        {
            Name = name;
            Koikoi = 0;
            Hand = new List<Card>();
            CollectedCards = new List<Card>();
            CollectedYaku = new List<KeyValuePair<Yaku, int>>();
            TotalPoints = 0;
        }

        public List<KeyValuePair<Yaku, int>> CollectedYaku
        {
            get { return _CollectedYaku; }
            set
            {
                _CollectedYaku = value;
                CalcPoints();
            }
        }

        public void Reset()
        {
            Hand.Clear();
            CollectedCards.Clear();
            CollectedYaku.Clear();
            tempPoints = 0;
            Koikoi = 0;
        }

        public void CalcPoints()
        {
            var nPoints = 0;
            Yaku.DistinctYakus(CollectedYaku);
            for (var i = 0; i < CollectedYaku.Count; i++)
            {
                var old = nPoints;
                nPoints += CollectedYaku[i].Key.basePoints;
                if (CollectedYaku[i].Key.addPoints != 0)
                    nPoints += (CollectedCards.Count(x => x.Typ == CollectedYaku[i].Key.TypPref) -
                                CollectedYaku[i].Key.minSize) * CollectedYaku[i].Key.addPoints;
                CollectedYaku[i] = new KeyValuePair<Yaku, int>(CollectedYaku[i].Key, nPoints - old);
            }

            tempPoints = nPoints + Koikoi;
        }

        [Command]
        public void CmdSetPoints(int value)
        {
            TotalPoints = value;
        }

        [Command]
        public void CmdAddKoiKoi()
        {
            Koikoi++;
        }
    }

    public class Spielfeld
    {
        public List<Card> Deck = new List<Card>();
        public bool isFinal;
        public KI.Move LastMove;
        public List<Card> Platz = new List<Card>();
        public List<object> players = new List<object>();
        public float Value;

        public Spielfeld(List<object> Players, int seed = -1)
        {
            players = Players;
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
            ((Player) players[0]).Hand.AddRange(((Player) parent.players[0]).Hand);
            ((Player) players[0]).CollectedCards.AddRange(((Player) parent.players[0]).CollectedCards);
            ((Player) players[0]).Koikoi = ((Player) parent.players[0]).Koikoi;
            ((Player) players[0]).tempPoints = ((Player) parent.players[0]).tempPoints;
            ((Player) players[1]).Hand.AddRange(((Player) parent.players[1]).Hand);
            ((Player) players[1]).CollectedCards.AddRange(((Player) parent.players[1]).CollectedCards);
            ((Player) players[1]).Koikoi = ((Player) parent.players[1]).Koikoi;
            ((Player) players[1]).tempPoints = ((Player) parent.players[1]).tempPoints;
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
                    ((Player) players[Turn ? 0 : 1]).CollectedCards.AddRange(matches);
                    break;
                case 2:
                    Platz.Remove(move.fCard);
                    ((Player) players[Turn ? 0 : 1]).CollectedCards.Add(move.fCard);
                    ((Player) players[Turn ? 0 : 1]).CollectedCards.Add(move.hCard);
                    break;
            }

            ((Player) players[Turn ? 0 : 1]).Hand.Remove(move.hCard);
            Platz.Add(Deck[0]);
            Deck.RemoveAt(0);
            var Yakus = new List<Yaku>();
            Yakus = Yaku.GetYaku(((Player) players[Turn ? 0 : 1]).CollectedCards.ToList());
            var nPoints = 0;
            for (var i = 0; i < Yakus.Count; i++)
            {
                nPoints += Yakus[i].basePoints;
                if (Yakus[i].addPoints != 0)
                    nPoints += (((Player) players[Turn ? 0 : 1]).CollectedCards.Count(x => x.Typ == Yakus[i].TypPref) -
                                Yakus[i].minSize) * Yakus[i].addPoints;
            }

            if (nPoints > ((Player) players[Turn ? 0 : 1]).tempPoints)
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
    }

    public class KI : Player
    {
        public enum Mode
        {
            Statistic,
            Searching,
            Omniscient
        }

        public Func<Spielfeld, int[]> MakeTurn;
        public Mode mode;
        public Spielfeld root;
        public List<List<Spielfeld>> StateTree = new List<List<Spielfeld>>();
        public object thisLock;
        private readonly List<Thread> threads = new List<Thread>();
        public bool Turn;

        public KI(Mode Modus, Spielfeld board, bool turn, string name) : base(name)
        {
            mode = Modus;
            Turn = turn;
            root = board;
            //Thread temp = new Thread(() => BuildStateTree());
            //temp.Start();
            switch (Modus)
            {
                case Mode.Omniscient:
                    /*
                     *  Umsetzung durch Bewertungsfunktion zuerst
                     */
                    MakeTurn = OmniscientCalcTurn;
                    break;
                case Mode.Searching:
                    /*
                     *  - Berechnung der Suchbäume von Spieler und Gegner separat -> 8!*2 statt 8!^2
                     *      -> Suchen des frühesten KoiKoi-Ausrufs
                     *      -> Einschränkung dieser Möglichkeiten basierend auf tatsächlichen Zügen
                     *      -> Mögliche Interaktion der beiden Bäume
                     *  - Problem: Unbekannte Deck-Karten -> Arbeit nur mit Feldkarten möglich
                     *  - Alternative: Zufälliger Zug aus verdeckten Karten -> sehr fehleranfällig!
                     */
                    break;
                case Mode.Statistic:
                    /*
                     *  Rekonstruierbare Informationen:
                     *  - Inhalt verdeckter Karten = Alle Karten - Hand - Feld - eigene Sammlung - gegnerische Sammlung
                     *      -> statistische Whkt für gegnerische Handkarten und gezogene Folgekarten
                     *  - Gegner: Erreichbare Yaku durch Summe der verdeckten Karten ermitteln
                     *      -> Konzentration auf Vermeidung dieser Zustände
                     *  - Wahrscheinlichkeit eines KoiKoi Ausrufs basierend auf bisheriger Punktelage und Zahl der Handkarten
                     *      - Beachte auch eigene Bedrohungslage an Hand von nahezu komplettierten Yaku
                     *  - Mögliche eigene Yakus und KoiKoi-Möglichkeiten an Hand von eigenen und verdeckten Karten ermitteln
                     *  - Verhindern von gegnerischen KoiKois durch gezieltes "wegschnappen" von Karten
                     *  - Besondere Beachtung vom Einsammeln durch Kartenzug -> Erweiterung der Möglichkeiten der Handkarten
                     *  -> Bewertungsfunktion
                     */
                    break;
            }
        }

        // Memo: optional Karte ziehen einbauen. variiert nach Spielmodus.
        public void BuildChildNodes(bool Turn, int level, int node)
        {
            // Memo: matches = 0
            // Memo: Koikoi sagen!
            var parent = StateTree[level][node];
            if (!parent.isFinal)
            {
                var aHand = ((Player) parent.players[Turn ? 0 : 1]).Hand.ToList();
                for (var i = 0; i < aHand.Count; i++)
                {
                    var matches = new List<Card>();
                    for (var j = 0; j < parent.Platz.Count; j++)
                        if (parent.Platz[j].Monat == aHand[i].Monat)
                            matches.Add(parent.Platz[j]);
                    if (matches.Count == 2)
                    {
                        for (var choice = 0; choice < 2; choice++)
                        {
                            var move = new Move(matches[choice], aHand[i]);
                            var child = new Spielfeld(parent, move, Turn);
                            child.LastMove = move;
                            StateTree[level + 1].Add(child);
                        }
                    }
                    else
                    {
                        var move = new Move(null, aHand[i]);
                        var child = new Spielfeld(parent, move, Turn);
                        child.LastMove = move;
                        StateTree[level + 1].Add(child);
                    }
                }
            }
        }

        // Memo: Konstruktion nur für einen Spieler einbauen: Jede 2. Karte ziehen.
        public void BuildStateTree(int maxDepth = 16)
        {
            StateTree.Clear();
            StateTree.Add(new List<Spielfeld> {root});
            for (var i = 0; i < maxDepth; i++)
                StateTree.Add(new List<Spielfeld>());
            for (var level = 0; level < maxDepth; level++, Turn = !Turn)
            {
                for (var node = 0; node < StateTree[level].Count; node++)
                {
                    while (threads.Count > 10000)
                        for (var i = 0; i < threads.Count; i++)
                            if (i == threads.Count - 1 && !threads[i].IsAlive)
                                threads.Clear();
                    var temp = new Thread(() => BuildChildNodes(Turn, level, node));
                    temp.Start();
                    threads.Add(temp);
                    Thread.Sleep(1);
                }

                var running = true;
                while (running)
                    for (var i = 0; i < threads.Count; i++)
                        if (threads[i].IsAlive)
                            break;
                        else if (i == threads.Count - 1 && !threads[i].IsAlive)
                            running = false;
                threads.Clear();
            }
        }

        public float OmniscientRateState(Spielfeld State)
        {
            /* Beachte:
             * - Koikoi = true? / isFinal?
             * - Unsicherheit des Gegners beachten!
             * - KI = player[1]
             * - Erweiterte Erreichbarkeit der Karten: Ziehbar, aber auch Einsammelbar?
             * - Blockade von durch Einsammeln von Karten (Einsammeln von wichtigen Deckkarten nicht mehr möglich)...
             */
            /* Wichtige Werte inklusive:
             * - Handkarten, Feldkarten, Deckkarten, Sammlung
             * - Temporäre Punkte
             * - Gesamtpunktzahl - Einfluss auf Koikoi
             */
            var oPossibleYaku = new List<Yaku>(); //NEU!
            var pPossibleYaku = new List<Yaku>();
            var oReachableCards = new List<Card>();
            var pReachableCards = new List<Card>();
            var oKoikoi = 0f;
            var oNextYakuWkt = 0f;
            float result = 0;
            var oCardsToYaku = 0;
            var pCardsToYaku = 0;
            oReachableCards.AddRange(State.Deck);
            oReachableCards.AddRange(((Player) State.players[0]).Hand);
            oReachableCards.AddRange(State.Platz);
            oReachableCards.AddRange(((Player) State.players[0]).CollectedCards);
            pReachableCards.AddRange(State.Deck.FindAll(x => State.Deck.IndexOf(x) % 2 == 0));
            pReachableCards.AddRange(((Player) State.players[1]).Hand);
            pReachableCards.AddRange(State.Platz);
            pReachableCards.AddRange(((Player) State.players[1]).CollectedCards);
            pPossibleYaku.AddRange(Yaku.GetYaku(pReachableCards));
            Yaku.DistinctYakus(pPossibleYaku);
            for (var i = 0; i < ((Player) State.players[1]).CollectedYaku.Count; i++)
            {
                var addPossible = ((Player) State.players[1]).CollectedYaku[i].Key.addPoints != 0;
                if (pReachableCards.Count(x => x.Typ == ((Player) State.players[1]).CollectedYaku[i].Key.TypPref) ==
                    Global.allCards.Count(x => x.Typ == ((Player) State.players[1]).CollectedYaku[i].Key.TypPref))
                    addPossible = false;
                pPossibleYaku.RemoveAll(x =>
                    x.name == ((Player) State.players[1]).CollectedYaku[i].Key.name && !addPossible);
            }

            oPossibleYaku.AddRange(Yaku.GetYaku(oReachableCards));
            Yaku.DistinctYakus(oPossibleYaku);
            for (var i = 0; i < ((Player) State.players[0]).CollectedYaku.Count; i++)
            {
                var addPossible = ((Player) State.players[0]).CollectedYaku[i].Key.addPoints != 0;
                if (oReachableCards.Count(x => x.Typ == ((Player) State.players[0]).CollectedYaku[i].Key.TypPref) ==
                    Global.allCards.Count(x => x.Typ == ((Player) State.players[0]).CollectedYaku[i].Key.TypPref))
                    addPossible = false;
                oPossibleYaku.RemoveAll(x =>
                    x.name == ((Player) State.players[0]).CollectedYaku[i].Key.name && !addPossible);
            }

            return result;
        }

        public float StochasticRateState(Spielfeld State)
        {
            return 0;
        }

        public int[] OmniscientCalcTurn(Spielfeld cRoot)
        {
            int[] result;
            Turn = false;
            root = cRoot;
            BuildStateTree(1);
            Debug.Log(StateTree.Count + "|" + StateTree[StateTree.Count - 1].Count);
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            for (var i = 0; i < StateTree[1].Count; i++)
            {
                StateTree[1][i].Value = OmniscientRateState(StateTree[1][i]);
                if (StateTree[1][i].Value > maxValue)
                {
                    maxValue = StateTree[1][i].Value;
                    selectedMove = StateTree[1][i].LastMove;
                }
            }

            var hCard = ((Player) cRoot.players[1]).Hand.FindIndex(x => x.name == selectedMove.hCard.name);
            result = new[]
            {
                hCard, selectedMove.fCard == null ? -1 : cRoot.Platz.FindIndex(x => x.name == selectedMove.fCard.name)
            };
            return result;
        }

        public int[] StatisticCalcTurn(Spielfeld root)
        {
            int[] result = { };
            return result;
        }

        public int[] SearchTurn()
        {
            int[] result = { };
            return result;
        }

        //Memo: Statt mit ganzen Klassen mit Indizes arbeiten?
        public class Move
        {
            public Card fCard;
            public Card fCard2;
            public Card hCard;
            public bool Koikoi;

            public Move(Card fieldCard, Card handCard, Card fieldCard2 = null, bool koikoi = false)
            {
                fCard = fieldCard;
                hCard = handCard;
                fCard2 = fieldCard2;
                Koikoi = koikoi;
            }
        }
    }
}