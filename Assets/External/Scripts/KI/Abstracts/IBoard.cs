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

        [Serializable]
        public struct Coords { public int x; public int y; }
        public Coords parentCoords;
        public Move LastMove;

        public void SayKoikoi(bool koikoi)
        {
            isFinal = !koikoi;
            LastMove.Koikoi = koikoi;
        }

        /// <summary>
        /// hard copy of reference board and variable initialization
        /// </summary>
        /// <param name="root"></param>
        public IBoard(Spielfeld root)
        {
            Turn = !root.Turn;
            Field = new List<Card>(root.Field);
            LastMove = null;
            computer = new Player(root.players[1 - Settings.PlayerID]);
            Value = 0f;
            isFinal = false;
        }

        protected IBoard(IBoard<T> board)
        {
            Turn = !board.Turn;
            Field = new List<Card>(board.Field);
            LastMove = null;
            computer = new Player(board.computer);
            Value = 0f;
            isFinal = board.isFinal;
        }

        public abstract T ApplyMove(Coords boardCoords, Move move, bool turn);
    }
}
