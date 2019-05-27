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
        public List<Card> computerCollection;
        public readonly Dictionary<int, Card> Deck;
        public List<int> CardsCollected;
        public int TurnID;

        public SearchingBoard() { }

        public SearchingBoard(SearchingBoard target)
        {
            Field = target.Field;
            Deck = target.Deck;
            playerHand = target.playerHand;
            computerHand = target.computerHand;
            computerCollection = target.computerCollection;
            CardsCollected = new List<int>(target.CardsCollected);
            Root = target.Root;
            TurnID = target.TurnID;
        }


        public SearchingBoard(Spielfeld root) : base(root)
        {
            playerHand = new Player(root.players[Settings.PlayerID]).Hand;
            computerHand = computer.Hand;
            computerCollection = computer.CollectedCards;
            Deck = root.Deck.ToDictionary(24);
            CardsCollected = new List<int>(8);
            Root = -1;
            TurnID = 0;
            Global.Log(string.Join(";",computerHand));
        }

        protected override void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn)
        {
            return;
        }

        protected override bool CheckYaku(bool turn)
        {
            return false;
        }

        public override SearchingBoard Clone()
        {
            SearchingBoard Result = new SearchingBoard(this);

            Result.Field = new List<Card>(Field);
            Result.computerHand = new List<Card>(computerHand);
            Result.computerCollection = new List<Card>(computerCollection);
            Result.CardsCollected = new List<int>(CardsCollected);
            Result.Turn = Turn;
            Result.Root = Root;
            Result.TurnID = TurnID;

            return Result;
        }
    }
}
