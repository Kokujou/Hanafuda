using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public abstract class IBoard<T> where T : IBoard<T>
    {
        public List<Card> Field;
        public Player computer;
        public bool HasNewYaku;
        public bool isFinal;
        public bool Turn;
        public float Value;
        public int Root;

        public Move LastMove;

        public T parent;

        public void SayKoikoi(bool koikoi)
        {
            isFinal = !koikoi;
            LastMove.Koikoi = koikoi;
        }

        public IBoard() { }

        /// <summary>
        /// hard copy of reference board and variable initialization
        /// </summary>
        /// <param name="root"></param>
        public IBoard(Spielfeld root)
        {
            Turn = root.Turn;
            Field = new List<Card>(root.Field);
            LastMove = null;
            computer = new Player(root.players[1 - Settings.PlayerID]);
            Value = 0f;
            isFinal = false;
        }

        protected IBoard(IBoard<T> board)
        {
            Turn = board.Turn;
            Field = new List<Card>(board.Field);
            LastMove = null;
            computer = new Player(board.computer);
            Value = 0f;
            isFinal = board.isFinal;
        }

        public virtual T ApplyMove(T parent, Move move, bool turn)
        {
            T board = parent.Clone();

            board.LastMove = move;
            board.parent = parent;
            
            board.ApplyMove(move.HandSelection, move.HandFieldSelection, true, turn);

            if (move.DeckSelection.Length > 0)
                board.ApplyMove(move.DeckSelection, move.DeckFieldSelection, false, turn);

            bool hasYaku = board.CheckYaku(turn);
            board.HasNewYaku = hasYaku;
            board.LastMove.HadYaku = hasYaku;

            return board;
        }
        public abstract T Clone();

        protected abstract void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn);
        protected abstract bool CheckYaku(bool turn);
    }
}
