using System;
using System.Collections.Generic;
using System.Linq;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

namespace Hanafuda
{
    public abstract class YakuCollection : List<YakuProperties>
    {
        protected Player player;
        protected Player opponent;

        protected VirtualBoard State;
        protected bool Turn;
        protected List<Card> NewCards;
        protected List<CardProperties> CardProps;
        protected CardCollection cardProperties;


        protected abstract void Preparations();
        protected abstract void CalcTargets(List<Card> newCards);
        protected abstract void CalcState(VirtualBoard State, bool Turn);
        protected abstract void CalcMinTurns(CardCollection cardProperties);
        protected abstract void CalcProbs(CardCollection cardProperties);

        public YakuCollection(List<CardProperties> list, List<Card> newCards, VirtualBoard state, bool turn) : base()
        {
            player = turn ? state.active : state.opponent;
            opponent = turn ? state.opponent : state.active;
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
                this.Add(new YakuProperties(yakuID));

            NewCards = newCards;
            State = state;
            Turn = turn;
            CardProps = list;

            Preparations();
            CalcTargets(newCards);
            CalcMinTurns(cardProperties);
            CalcState(state, turn);
            CalcProbs(cardProperties);
            Global.Log(string.Join(";",this.Select(x=>x.Probability.ToString())));
        }
    }
}