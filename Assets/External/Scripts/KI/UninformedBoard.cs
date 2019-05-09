using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class UninformedBoard : IBoard<UninformedBoard>
    {
        public List<KeyValuePair<Card, float>> UnknownCards;
        public List<Card> OpponentCollection;

        public UninformedBoard(Spielfeld root) : base(root)
        {
            OpponentCollection = root.players[1].CollectedCards;
            UnknownCards = Global.allCards
                .Except(OpponentCollection)
                .Except(active.Hand)
                .Except(active.CollectedCards)
                .Except(Deck)
                .Except(Field)
                .ToDictionary(x => x, x => 0f)
                .ToList();
        }

        protected UninformedBoard(UninformedBoard target) : base(target)
        {
            UnknownCards = new List<KeyValuePair<Card, float>>(UnknownCards);
            OpponentCollection = new List<Card>(target.OpponentCollection);
        }

        public override UninformedBoard ApplyMove(Coords boardCoords, Move move, bool turn)
        {
            return null;
        }
    }
}
