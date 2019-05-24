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
                UninformedBoard parent = (UninformedBoard)param;
                List<UninformedBoard> result = new List<UninformedBoard>();

                if (!parent.isFinal)
                {
                    Dictionary<Card, float> aHand = parent.Turn ? parent.computer.Hand.ToDictionary(x => x, x => 1f) : parent.UnknownCards;
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
                        else
                            ToBuild.Add(move);
                        for (int build = 0; build < ToBuild.Count; build++)
                        {
                            UninformedBoard child = parent.ApplyMove(parent, ToBuild[build], parent.Turn);
                            if (child.HasNewYaku)
                            {
                                child.SayKoikoi(true);
                                UninformedBoard finalChild = parent.ApplyMove(parent, ToBuild[build], parent.Turn);
                                finalChild.SayKoikoi(false);
                                result.Add(finalChild);
                            }
                            result.Add(child);
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