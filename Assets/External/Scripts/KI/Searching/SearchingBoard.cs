using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class SearchingBoard : IBoard<SearchingBoard>
    {
        public List<Card> playerHand;
        public List<Card> computerHand;
        public List<Card> playerCollection;
        public List<Card> computerCollection;
        public List<Card> Deck;
        public SearchingBoard() { }

        public SearchingBoard(SearchingBoard target)
        {
            Field = target.Field;
            Deck = target.Deck;
            playerHand = target.playerHand;
            computerHand = target.computerHand;
            playerCollection = target.playerCollection;
            computerCollection = target.computerCollection;
        }

        public SearchingBoard(Spielfeld root) : base(root)
        {
            playerHand = new List<Card>(root.players[1 - Settings.PlayerID].Hand);
            computerHand = computer.Hand;
            playerCollection = new List<Card>(root.players[1 - Settings.PlayerID].CollectedCards);
            computerCollection = computer.CollectedCards;
            Deck = new List<Card>(root.Deck);
        }

        protected override void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn)
        {
            return;
        }

        protected override bool CheckYaku(bool turn)
        {
            return false;
        }

        protected override SearchingBoard Clone() => new SearchingBoard(this);
    }
}
