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
        private readonly List<Task> tasks = new List<Task>();

        [Serializable]
        public class NodeParameters
        {
            public int level;
            public int node;
            public bool turn;
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
        private void BuildChildNodes(object param)
        {
            Console.WriteLine("Test");
            NodeParameters parameters = (NodeParameters)param;
            int level = parameters.level;
            bool turn = parameters.turn;
            VirtualBoard parent = StateTree[level][parameters.node];
            // Memo: matches = 0
            // Memo: Koikoi sagen!
            if (!parent.isFinal)
            {
                var aHand = ((Player)parent.players[turn ? 1- Settings.PlayerID : Settings.PlayerID]).Hand;
                for (var i = 0; i < aHand.Count; i++)
                {
                    List<Move> ToBuild = new List<Move>();
                    Move move = new Move();
                    move.HandSelection = aHand[i].Title;
                    List<Card> handMatches = parent.Field.FindAll(x => x.Monat == aHand[i].Monat);
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
                        VirtualBoard child = new VirtualBoard(parent, ToBuild[build], turn);
                        child.LastMove = ToBuild[build];
                        if (child.HasNewYaku)
                        {
                            child.SayKoikoi(true);
                            VirtualBoard finalChild = new VirtualBoard(parent, ToBuild[build], turn);
                            finalChild.LastMove = ToBuild[build];
                            finalChild.SayKoikoi(false);
                            lock (thisLock)
                                StateTree[level + 1].Add(finalChild);
                        }
                        lock (thisLock)
                            StateTree[level + 1].Add(child);
                    }
                }
            }
        }

        // Memo: Konstruktion nur für einen Spieler einbauen: Jede 2. Karte ziehen.
        public void BuildStateTree(int maxDepth = 16, bool Turn = true)
        {
            StateTree.Clear();
            StateTree.Add(new List<VirtualBoard> { root });
            for (var i = 0; i < maxDepth; i++)
                StateTree.Add(new List<VirtualBoard>());
            for (var level = 0; level < maxDepth; level++, Turn = !Turn)
            {
                for (var node = 0; node < StateTree[level].Count; node++)
                {
                    while (tasks.Count > 10000)
                        for (var i = 0; i < tasks.Count; i++)
                            if (!tasks[i].IsCompleted)
                                break;
                            else if (i == tasks.Count - 1)
                                tasks.Clear();
                    Task temp = new TaskFactory().StartNew(x => BuildChildNodes(x), new NodeParameters() { level = level, node = node, turn = Turn });
                    tasks.Add(temp);
                }
                Task.WaitAll(tasks.ToArray());
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
