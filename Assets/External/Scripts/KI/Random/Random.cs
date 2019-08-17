using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class RandomAI : KI<OmniscientBoard>
    {
        public RandomAI(string name) : base(name) { }
        public override Dictionary<string, float> GetWeights() => new Dictionary<string, float>();

        public override float RateState(OmniscientBoard state) => 0f;

        public override Move RequestDeckSelection(IHanafudaBoard board, Move baseMove, int playerID) => baseMove;

        public override void SetWeight(string name, float value) { }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID) { }

        public override Move MakeTurn(IHanafudaBoard board, int playerID)
        {
            System.Random rnd = new System.Random();
            OmniscientBoard currentBoard = new OmniscientBoard(board, playerID);
            Move move = new Move();

            List<Card> newCollection = new List<Card>(currentBoard.computer.CollectedCards);

            int handID = rnd.Next(0, currentBoard.computer.Hand.Count);
            Card handSelection = currentBoard.computer.Hand[handID];
            move.HandSelection = handSelection.Title;
            List<Card> handMatches = currentBoard.Field.FindAll(x => x.Monat == handSelection.Monat);
            if (handMatches.Count == 2)
            {
                Card handFieldSelection = handMatches[rnd.Next(0, 2)];
                newCollection.Add(handSelection);
                newCollection.Add(handFieldSelection);
                move.HandFieldSelection = handFieldSelection.Title;
            }
            else if (handMatches.Count != 0)
            {
                newCollection.AddRange(handMatches);
                newCollection.Add(handSelection);
            }

            Card deckSelection = currentBoard.Deck[0];
            move.DeckSelection = deckSelection.Title;
            List<Card> deckMatches = currentBoard.Field.FindAll(x => x.Monat == deckSelection.Monat);
            if (deckMatches.Count == 2)
            {
                Card deckFieldSelection = deckMatches[rnd.Next(0, 2)];
                newCollection.Add(deckSelection);
                newCollection.Add(deckFieldSelection);
                move.DeckFieldSelection = deckFieldSelection.Title;
            }
            else if (deckMatches.Count != 0)
            {
                newCollection.AddRange(deckMatches);
                newCollection.Add(deckSelection);
            }

            if (Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), newCollection).Count > 0)
                move.HadYaku = true;

            move.Koikoi = false;

            return move;
        }
    }
}
