using System;
using System.Collections.Generic;

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

        protected abstract CardCollection cardProperties { get; }

        protected Action Preparations = () => { };
        protected abstract void CalcTargets(List<Card> newCards);
        protected abstract void CalcState(VirtualBoard State, bool Turn);
        protected abstract void CalcMinTurns(CardCollection cardProperties);
        protected abstract void CalcProbs(CardCollection cardProperties);

        public YakuCollection(List<CardProperties> list, List<Card> newCards, VirtualBoard State, bool Turn) : base()
        {
            player = State.players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID];
            opponent = State.players[Turn ? Settings.PlayerID : 1 - Settings.PlayerID];
            Preparations();
            CalcTargets(newCards);
            CalcState(State, Turn);
            CalcMinTurns(cardProperties);
            CalcProbs(cardProperties);
        }
    }
}