using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class KI : Player
    {
        public Move OmniscientCalcTurn(VirtualBoard cRoot)
        {
            root = cRoot;
            root.Turn = true;
            BuildStateTree(1);
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            for (var i = 0; i < StateTree[1].Count; i++)
            {
                StateTree[1][i].Value = OmniscientRateState(StateTree[1][i]);
                if (StateTree[1][i].Value > maxValue)
                {
                    maxValue = StateTree[1][i].Value;
                    selectedMove = StateTree[1][i].LastMove;
                }
            }
            return selectedMove;
        }
        public float OmniscientRateState(VirtualBoard State)
        {
            /* Beachte:
             * - Koikoi = true? / isFinal?
             * - Unsicherheit des Gegners beachten!
             * - KI = player[1]
             * - Erweiterte Erreichbarkeit der Karten: Ziehbar, aber auch Einsammelbar?
             * - Blockade von durch Einsammeln von Karten (Einsammeln von wichtigen Deckkarten nicht mehr möglich)...
             */
            /* Wichtige Werte inklusive:
             * - Handkarten, Feldkarten, Deckkarten, Sammlung
             * - Temporäre Punkte
             * - Gesamtpunktzahl - Einfluss auf Koikoi
             */
            var oPossibleYaku = new List<Yaku>(); //NEU!
            var pPossibleYaku = new List<Yaku>();
            var oReachableCards = new List<Card>();
            var pReachableCards = new List<Card>();
            var oKoikoi = 0f;
            var oNextYakuWkt = 0f;
            float result = 0;
            var oCardsToYaku = 0;
            var pCardsToYaku = 0;
            return result;
            oReachableCards.AddRange(State.Deck);
            oReachableCards.AddRange(((Player)State.players[0]).Hand);
            oReachableCards.AddRange(State.Field);
            oReachableCards.AddRange(((Player)State.players[0]).CollectedCards);
            pReachableCards.AddRange(State.Deck.FindAll(x => State.Deck.IndexOf(x) % 2 == 0));
            pReachableCards.AddRange(((Player)State.players[1]).Hand);
            pReachableCards.AddRange(State.Field);
            pReachableCards.AddRange(((Player)State.players[1]).CollectedCards);
            pPossibleYaku.AddRange(Yaku.GetYaku(pReachableCards));
            Yaku.DistinctYakus(pPossibleYaku);
            for (var i = 0; i < ((Player)State.players[1]).CollectedYaku.Count; i++)
            {
                var addPossible = ((Player)State.players[1]).CollectedYaku[i].Key.addPoints != 0;
                if (pReachableCards.Count(x => x.Typ == ((Player)State.players[1]).CollectedYaku[i].Key.TypPref) ==
                    Global.allCards.Count(x => x.Typ == ((Player)State.players[1]).CollectedYaku[i].Key.TypPref))
                    addPossible = false;
                pPossibleYaku.RemoveAll(x =>
                    x.Title == ((Player)State.players[1]).CollectedYaku[i].Key.Title && !addPossible);
            }

            oPossibleYaku.AddRange(Yaku.GetYaku(oReachableCards));
            Yaku.DistinctYakus(oPossibleYaku);
            for (var i = 0; i < ((Player)State.players[0]).CollectedYaku.Count; i++)
            {
                var addPossible = ((Player)State.players[0]).CollectedYaku[i].Key.addPoints != 0;
                if (oReachableCards.Count(x => x.Typ == ((Player)State.players[0]).CollectedYaku[i].Key.TypPref) ==
                    Global.allCards.Count(x => x.Typ == ((Player)State.players[0]).CollectedYaku[i].Key.TypPref))
                    addPossible = false;
                oPossibleYaku.RemoveAll(x =>
                    x.Title == ((Player)State.players[0]).CollectedYaku[i].Key.Title && !addPossible);
            }

            return result;
        }
    
    }
}