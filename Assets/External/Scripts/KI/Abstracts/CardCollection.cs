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

        protected OmniscientBoard State;
        protected bool Turn;

        protected abstract void Preparations();
        protected abstract void CalcMinTurns(OmniscientBoard State, bool Turn);
        protected abstract void CalcProbs(OmniscientBoard State, bool Turn);


        public CardCollection(IEnumerable<CardProperties> list, OmniscientBoard state, bool turn) : base(list)
        {
            player = turn ? state.computer : state.player;
            opponent = turn ? state.player : state.computer;
            State = state;
            Turn = turn;

            Preparations();
            CalcMinTurns(state, turn);
            CalcProbs(state, turn);
        }
    }
}