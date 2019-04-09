using System;
using System.Collections.Generic;

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
            player = state.players[turn ? 1 - Settings.PlayerID : Settings.PlayerID];
            opponent = state.players[turn ? Settings.PlayerID : 1 - Settings.PlayerID];
            State = state;
            Turn = turn;

            Preparations();
            CalcMinTurns(state, turn);
            CalcProbs(state, turn);
        }
    }
}