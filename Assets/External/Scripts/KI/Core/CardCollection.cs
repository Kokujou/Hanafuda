using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public abstract class CardCollection<T> : List<CardProperties> where T : IBoard<T>
    {
        protected T State;
        protected bool Turn;

        protected abstract void Preparations();
        protected abstract void CalcMinTurns(T State, bool Turn);
        protected abstract void CalcProbs(T State, bool Turn);


        public CardCollection(IEnumerable<CardProperties> list, T state, bool turn) : base(list)
        {
            State = state;
            Turn = turn;
            Preparations();

            CalcMinTurns(state, turn);
            CalcProbs(state, turn);
        }
    }
}