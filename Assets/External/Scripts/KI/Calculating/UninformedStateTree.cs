using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public partial class CalculatingAI
    {
        public class UninformedStateTree : IStateTree<UninformedBoard>
        {
            protected override object BuildChildNodes(object param)
            {
                NodeParameters parameters = (NodeParameters)param;
                int level = parameters.level;
                int node = parameters.node;
                bool turn = parameters.turn;
                UninformedBoard parent = Content[level][parameters.node];
                NodeReturn result = new NodeReturn();
                result.level = level;
                result.turn = turn;

                if (!parent.isFinal)
                {
                    Dictionary<Card, float> aHand = turn ? parent.UnknownCards : parent.computer.Hand.ToDictionary(x => x, x => 1f);
                    for (int handID = 0; handID < aHand.Count; handID++)
                    {
                        List<Move> ToBuild = new List<Move>();
                        Move move = new Move();
                        move.HandSelection = aHand.ElementAt(handID).Key.Title;
                        List<Card> handMatches = new List<Card>();
                        for (int field = 0; field < parent.Field.Count; field++)
                        {
                            if (parent.Field[field].Monat == aHand.ElementAt(handID).Key.Monat)
                                handMatches.Add(parent.Field[field]);
                        }
                        if (handMatches.Count == 2)
                        {
                            for (var handChoice = 0; handChoice < 2; handChoice++)
                            {
                                Move handMove = new Move(move);
                                handMove.HandFieldSelection = handMatches[handChoice].Title;
                                ToBuild.Add(handMove);
                            }
                        }
                        for (int build = 0; build < ToBuild.Count; build++)
                        {
                            UninformedBoard child = parent.ApplyMove(new UninformedBoard.Coords { x = level, y = node }, ToBuild[build], turn);
                            if (child.HasNewYaku)
                            {
                                child.SayKoikoi(true);
                                UninformedBoard finalChild = parent.ApplyMove(new UninformedBoard.Coords { x = level, y = node }, ToBuild[build], turn);
                                finalChild.SayKoikoi(false);
                                result.states.Add(finalChild);
                            }
                            result.states.Add(child);
                        }
                    }
                }

                return result;
            }

            public override void Build(int maxDepth = 16, bool Turn = true, bool SkipOpponent = false) => base.Build(maxDepth, Turn, SkipOpponent);

            public UninformedStateTree(UninformedBoard root = null, List<List<UninformedBoard>> tree = null) : base(root, tree) { }
        }
    }
}