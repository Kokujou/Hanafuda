﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public abstract class IBoard<T> where T : IBoard<T>
    {
        public List<Card> Deck, Field;
        public Player active;
        public bool HasNewYaku;
        public bool isFinal;
        public bool Turn;
        public float Value;

        [Serializable]
        public struct Coords { public int x; public int y; }
        public Coords parentCoords;
        public Move LastMove;

        /// <summary>
        /// hard copy of reference board and variable initialization
        /// </summary>
        /// <param name="root"></param>
        public IBoard(Spielfeld root)
        {
            Deck = new List<Card>(root.Deck);
            Field = new List<Card>(root.Field);
            LastMove = null;
            active = new Player(root.players[1]);
            Value = 0f;
            isFinal = false;
        }

        protected IBoard(IBoard<T> board)
        {
            Deck = new List<Card>(board.Deck);
            Field = new List<Card>(board.Field);
            LastMove = null;
            active = new Player(board.active);
            Value = 0f;
            isFinal = board.isFinal;
        }

        public abstract T ApplyMove(Coords boardCoords, Move move);
    }
}
