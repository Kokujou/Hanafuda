using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/*
 * Theoretischer Zustandsbaum:
 *  - Mögliche Kombination mit unwissender mathematischer KI im ersten Zug!
 *  - Basis: Zusammenstellung aller Yaku bzgl. bekannter und unbekannter Karten
 *  - In jedem Zug: 
 *      - Berechnung aller erreichbaren Karten inklusive deren Wahrscheinlichkeit
 *          - Über Berechnung von Sammelwahrscheinlichkeit von Tupeln <Monat; 0,2,4 eingesammelt> 
 *              - Wahrscheinlichkeit der gespielten Karte eingesammelt = 1, wenn > 0
 *              - Erhöhte Wahrscheinlichkeit für alle ursprünglich auf dem feld liegenden Karten
 *                  -> Sinkende Wahrscheinlichkeit für große Baumlänge
 *          - Kombination meherer Monats-Einträge über Produktregel
 *          - Beachtung von Zügen vom Deck!
 *      - Anschließend: Berechnung der daraus resultierenden Yaku inklusive Wahrscheinlichkeit
 *      - Entgültiger Wert: Wahrscheinlichkeit eines Yaku in diesem Zug
 *          - Beanchte: Addpoints, Whkt einen Yaku mehrfach zu erzielen!
 *  - Weiterverfolgung eines Zuges, wenn:
 *      - Die neu erreichten Karten zu Yaku mit hoher Gesamtwahrscheinlichkeit gehören
 *          - Bsp: Kasu ausschließen
 *  - Qualität eines direkten Folgezuges: Gesamtwahrscheinlichkeit einen Yaku zu erzielen vor dem Gegner
 *  - Für Gegner:
 *      - Berechne Alle erreichbaren Yaku aus erreichbaren Karten
 *      - Dann berechne Monate, die mindestens in einem Yaku enthalten sind -> Mindestdauer
 *  - Zusatz: In jedem Zug: abziehen von sehr wahrscheinlichen Karten vom Gegnerpool
 *      -> Neuberechnung der Mindesdauer
 *  - In jedem Zug: Mindestdauer bis Yaku für Gegner
 *      - Gewicht verringern, wenn Eigene Yakudauer länger (?)
 *  - Nullzüge werden ignoriert, da unsinnig und unwahrscheinlich
 */

/*
 * Vollständiger Zustandsbaum:
 *  - Nutzung eines echten Spielfeldes
 *  - Vermeidung des vollständigen Aufbaus, sondern lediglich bis Tiefe 1 oder 2 (mit Gegner)
 *  
 */

namespace Hanafuda
{
    public class StateTree
    {
        public VirtualBoard Root;
        public int Size => Content.Count;
        public List<VirtualBoard> GetLevel(int id) => Content[id];
        public VirtualBoard GetState(int x, int y) => Content[x][y];

        private List<List<VirtualBoard>> Content = new List<List<VirtualBoard>>();
        private object thisLock;
        private readonly List<Task<object>> tasks = new List<Task<object>>();

        [Serializable]
        public class NodeParameters
        {
            public int level;
            public int node;
            public bool turn;
        }
        [Serializable]
        public class NodeReturn
        {
            public int level;
            public bool turn;
            public List<VirtualBoard> states = new List<VirtualBoard>();
        }

        private static List<Move> AddDeckActions(List<Card> deckMatches, Move root)
        {
            List<Move> ToBuild = new List<Move>();
            if (deckMatches.Count == 2)
            {
                for (var deckChoice = 0; deckChoice < 2; deckChoice++)
                {
                    Move deckMove = new Move(root);
                    deckMove.DeckFieldSelection = deckMatches[deckChoice].Title;
                    ToBuild.Add(deckMove);
                }
            }
            else
                ToBuild.Add(root);
            return ToBuild;
        }

        private object BuildChildNodes(object param)
        {
            NodeParameters parameters = (NodeParameters)param;
            int level = parameters.level;
            int node = parameters.node;
            bool turn = parameters.turn;
            VirtualBoard parent = Content[level][parameters.node];
            NodeReturn result = new NodeReturn();
            result.level = level;
            result.turn = turn;
            // Memo: matches = 0
            // Memo: Koikoi sagen!
            if (!parent.isFinal)
            {
                List<Card> aHand = turn ? parent.active.Hand : parent.opponent.Hand;
                for (var i = 0; i < aHand.Count; i++)
                {
                    List<Move> ToBuild = new List<Move>();
                    Move move = new Move();
                    move.HandSelection = aHand[i].Title;
                    move.DeckSelection = parent.Deck[0].Title;
                    List<Card> handMatches = new List<Card>();
                    List<Card> deckMatches = new List<Card>();
                    for (int field = 0; field < parent.Field.Count; field++)
                    {
                        if (parent.Field[field].Monat == aHand[i].Monat)
                            handMatches.Add(parent.Field[field]);
                        if (parent.Field[field].Monat == parent.Deck[0].Monat)
                            deckMatches.Add(parent.Field[field]);
                    }
                    Global.Log(string.Join(",", parent.Field));
                    if (handMatches.Count == 2)
                    {
                        for (var handChoice = 0; handChoice < 2; handChoice++)
                        {
                            Move handMove = new Move(move);
                            handMove.HandFieldSelection = handMatches[handChoice].Title;
                            ToBuild.AddRange(AddDeckActions(deckMatches, handMove));
                        }
                    }
                    else ToBuild.AddRange(AddDeckActions(deckMatches, move));
                    for (int build = 0; build < ToBuild.Count; build++)
                    {
                        VirtualBoard child = parent.ApplyMove(new VirtualBoard.Coords { x = level, y = node }, ToBuild[build], turn);
                        if (child.HasNewYaku)
                        {
                            child.SayKoikoi(true);
                            VirtualBoard finalChild = parent.ApplyMove(new VirtualBoard.Coords { x = level, y = node }, ToBuild[build], turn);
                            finalChild.SayKoikoi(false);
                            result.states.Add(finalChild);
                        }
                        result.states.Add(child);
                    }
                }
            }
            return result;
        }

        // Memo: Konstruktion nur für einen Spieler einbauen: Jede 2. Karte ziehen.
        public void Build(int maxDepth = 16, bool Turn = true, bool SkipOpponent = false)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Content.Clear();
            Content.Add(new List<VirtualBoard> { Root });
            for (var i = 0; i < maxDepth; i++)
                Content.Add(new List<VirtualBoard>());
            Task<object> firstTask = new Task<object>(x => BuildChildNodes(x), (new NodeParameters() { level = 0, node = 0, turn = Turn }));
            firstTask.Start();
            tasks.Add(firstTask);
            while (tasks.Count > 0 && Content.Last().Count == 0)
            {
                Task.WaitAny(tasks.ToArray());
                for (int task = tasks.Count - 1; task >= 0; task--)
                {
                    if (tasks[task].IsCompleted)
                    {
                        NodeReturn result = (NodeReturn)tasks[task].Result;
                        tasks.RemoveAt(task);
                        Content[result.level + 1].AddRange(result.states);
                        if (result.level + 1 >= maxDepth) continue;
                        for (int i = 0; i < result.states.Count; i++)
                        {
                            Task<object> newTask = new Task<object>(x => BuildChildNodes(x), (object)new NodeParameters() { level = result.level + 1, node = Content[result.level + 1].Count - (i + 1), turn = SkipOpponent ? Turn : !result.turn });
                            newTask.Start();
                        }
                    }
                }

            }
            //process.StandardInput.WriteLine($"Time to Completion: {watch.Elapsed.TotalSeconds} ^");
            /*
            GameObject text = GameObject.Instantiate(Global.prefabCollection.PText);
            text.GetComponentsInChildren<TextMesh>()[0].text = watch.Elapsed.TotalSeconds.ToString();
            text.GetComponentsInChildren<TextMesh>()[1].text = watch.Elapsed.TotalSeconds.ToString();*/
        }

        public StateTree(VirtualBoard root = null, List<List<VirtualBoard>> tree = null)
        {
            thisLock = new object();
            if (root != null)
                Root = root;
            if (tree != null)
                Content = tree;
        }
        public static implicit operator StateTree(List<List<VirtualBoard>> target)
        {
            return new StateTree(target[0][0], target);
        }
    }
}
