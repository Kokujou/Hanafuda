using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public abstract class CardCollection : List<CardProperties>
    {
        protected Player player;
        protected Player opponent;

        protected VirtualBoard State;
        protected bool Turn;

        protected abstract void Preparations();
        protected abstract void CalcMinTurns(VirtualBoard State, bool Turn);
        protected abstract void CalcProbs(VirtualBoard State, bool Turn);


        public CardCollection(IEnumerable<CardProperties> list, VirtualBoard state, bool turn) : base(list)
        {
            player = turn ? state.active : state.opponent;
            opponent = turn ? state.opponent : state.active;
            State = state;
            Turn = turn;

            Preparations();
            CalcMinTurns(state, turn);
            CalcProbs(state, turn);
        }
    }
}