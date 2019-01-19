using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class KI
    {
        public VirtualBoard root;
        public List<List<VirtualBoard>> StateTree = new List<List<VirtualBoard>>();
        public object thisLock;
        private readonly List<Task<object>> tasks = new List<Task<object>>();
        private System.Diagnostics.Process process;

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

        private async Task<List<VirtualBoard>> BoardFromMove(Move move, object param)
        {
            List<VirtualBoard> result = new List<VirtualBoard>();
            NodeParameters parameters = (NodeParameters)param;
            int level = parameters.level;
            int node = parameters.node;
            bool turn = parameters.turn;
            VirtualBoard parent = StateTree[level][parameters.node];
            VirtualBoard child = new VirtualBoard(parent, move, turn, node);
            if (child.HasNewYaku)
            {
                child.SayKoikoi(true);
                VirtualBoard finalChild = new VirtualBoard(parent, move, turn, node);
                finalChild.SayKoikoi(false);
                result.Add(finalChild);
            }
            result.Add(child);
            return result;
        }

        private async Task<List<VirtualBoard>> BuildNode(Card HandSelection, object param)
        {
            List<Move> ToBuild = new List<Move>();
            Move move = new Move();
            NodeParameters parameters = (NodeParameters)param;
            List<VirtualBoard> result = new List<VirtualBoard>();
            VirtualBoard parent = StateTree[parameters.level][parameters.node];
            move.HandSelection = HandSelection.Title;
            List<Card> handMatches = parent.Field.FindAll(x => x.Monat == HandSelection.Monat);
            move.DeckSelection = parent.Deck[0].Title;
            List<Card> deckMatches = parent.Field.FindAll(x => x.Monat == parent.Deck[0].Monat);
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
                result.AddRange(await BoardFromMove(ToBuild[build], param));
            }
            return result;
        }

        private object BuildChildNodes(object param)
        {
            Console.WriteLine("Test");
            NodeParameters parameters = (NodeParameters)param;
            int level = parameters.level;
            int node = parameters.node;
            bool turn = parameters.turn;
            VirtualBoard parent = StateTree[level][parameters.node];
            NodeReturn result = new NodeReturn();
            result.level = level;
            result.turn = turn;
            // Memo: matches = 0
            // Memo: Koikoi sagen!
            if (!parent.isFinal)
            {
                var aHand = ((Player)parent.players[turn ? 1 - Settings.PlayerID : Settings.PlayerID]).Hand;
                List<Task<List<VirtualBoard>>> tasks = new List<Task<List<VirtualBoard>>>();
                for (var i = 0; i < aHand.Count; i++)
                {
                    tasks.Add(BuildNode(aHand[i], param));
                }
                Task.WaitAll(tasks.ToArray());
                for (int i = 0; i < tasks.Count; i++)
                    result.states.AddRange(tasks[i].Result);
            }
            return result;
        }

        // Memo: Konstruktion nur für einen Spieler einbauen: Jede 2. Karte ziehen.
        public void BuildStateTree(int maxDepth = 16, bool Turn = true, bool SkipOpponent = false)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "CMD.EXE", RedirectStandardInput = true, UseShellExecute = false });
            process.StandardInput.WriteLine("echo off");
            process.StandardInput.WriteLine("cls");
            StateTree.Clear();
            StateTree.Add(new List<VirtualBoard> { root });
            for (var i = 0; i < maxDepth; i++)
                StateTree.Add(new List<VirtualBoard>());
            Task<object> firstTask = new Task<object>(x => BuildChildNodes(x), (new NodeParameters() { level = 0, node = 0, turn = Turn }));
            firstTask.Start();
            tasks.Add(firstTask);
            List<Task<object>> pendingTasks = new List<Task<object>>();
            while (tasks.Count > 0 && StateTree.Last().Count == 0)
            {
                Task.WaitAny(tasks.ToArray());
                List<Task<object>> newTasks = new List<Task<object>>();
                for (int task = tasks.Count - 1; task >= 0; task--)
                {
                    if (tasks[task].IsCompleted)
                    {
                        NodeReturn result = (NodeReturn)tasks[task].Result;
                        tasks.RemoveAt(task);
                        StateTree[result.level + 1].AddRange(result.states);
                        for (int i = 0; i < result.states.Count; i++)
                        {
                            Task<object> newTask = new Task<object>(x => BuildChildNodes(x), (object)new NodeParameters() { level = result.level + 1, node = StateTree[result.level + 1].Count - (i + 1), turn = SkipOpponent ? Turn : !result.turn });
                            if (tasks.Count < 8)
                            {
                                newTask.Start();
                                newTasks.Add(newTask);
                            }
                            else
                                pendingTasks.Add(newTask);
                        }
                        int last = 0;
                        for (int i = StateTree.Count - 1; i >= 0; i--)
                            if (StateTree[i].Count > 0)
                            {
                                last = i;
                                break;
                            }
                    }
                    if(tasks.Count < 8)
                    {
                        for (int i = 0; i < pendingTasks.Count; i++)
                            pendingTasks[i].Start();
                        tasks.AddRange(pendingTasks);
                        pendingTasks = new List<Task<object>>();
                    }
                }
                tasks.AddRange(newTasks);

            }
            process.StandardInput.WriteLine($"Time to Completion: {watch.Elapsed.TotalSeconds} ^");

            /*for (var level = 0; level < maxDepth; level++, Turn = !Turn)
            {
                for (var node = 0; node < StateTree[level].Count; node++)
                {
                    tasks.Add(BuildChildNodes(new NodeParameters() { level = level, node = node, turn = Turn }));
                }
                await Task.WhenAll(tasks);
            }*/
        }
    }
}
