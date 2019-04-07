using System;
using System.Collections.Generic;

namespace Hanafuda
{
    public abstract class CardCollection : List<CardProperties>
    {
        protected Player player;
        protected Player opponent;

        protected Action Preparations = () => { };
        protected abstract void CalcMinTurns(VirtualBoard State, bool Turn);
        protected abstract void CalcProbs(VirtualBoard State, bool Turn);


        public CardCollection(IEnumerable<CardProperties> list, VirtualBoard State, bool Turn) : base(list)
        {
            player = State.players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID];
            opponent = State.players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID];

            Preparations();
            CalcMinTurns(State, Turn);
            CalcProbs(State, Turn);
        }
    }
}