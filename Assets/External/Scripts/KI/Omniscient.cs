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
             * 
             * Grundidee: Whkt des Erreichens eines Yaku vor dem Gegner
             */

            if (State.isFinal) return 1;
            float result = 0f;

            int RoundsLeft = 0;

            //Beachte: Übereinstimmung mit den max. 8 Monaten aus der Hand!
            List<Card> oReachableCards = new List<Card>();
            List<Card> pReachableCards = new List<Card>();

            // Abzüglich Deckkarten da für Gegner unbekannt -> unwichtig für Strategie
            List<Yaku> oPossibleYaku = new List<Yaku>();
            List<Yaku> pPossibleYaku = new List<Yaku>();

            //Gegnerische Karten, die nicht mit Deck oder Feld gepaart werden können
            List<Card> OCardsToField = new List<Card>();

            //Karten die vor Spielende gezogen werden
            List<Card> ReachableDeckCards = new List<Card>();

            List<Card> CollectedCards = new List<Card>();

            float oKoikoi = 0f;

            int pNextYakuIn = 0;
            int oNextYakuIn = 0;

            int pPoints = 0;
            int oPoints = 0;
            
            // Wenn 0: Gegner muss Karte aufs Feld legen -> Erhöhung erreichbarer Karten
            int oPlayableCards = 0;

            bool DeckIntervenes = false;

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
                if (pReachableCards.Count(x => x.Typ == ((Player)State.players[1]).CollectedYaku[i].Key.TypePref) ==
                    Global.allCards.Count(x => x.Typ == ((Player)State.players[1]).CollectedYaku[i].Key.TypePref))
                    addPossible = false;
                pPossibleYaku.RemoveAll(x =>
                    x.Title == ((Player)State.players[1]).CollectedYaku[i].Key.Title && !addPossible);
            }

            oPossibleYaku.AddRange(Yaku.GetYaku(oReachableCards));
            Yaku.DistinctYakus(oPossibleYaku);
            for (var i = 0; i < ((Player)State.players[0]).CollectedYaku.Count; i++)
            {
                var addPossible = ((Player)State.players[0]).CollectedYaku[i].Key.addPoints != 0;
                if (oReachableCards.Count(x => x.Typ == ((Player)State.players[0]).CollectedYaku[i].Key.TypePref) ==
                    Global.allCards.Count(x => x.Typ == ((Player)State.players[0]).CollectedYaku[i].Key.TypePref))
                    addPossible = false;
                oPossibleYaku.RemoveAll(x =>
                    x.Title == ((Player)State.players[0]).CollectedYaku[i].Key.Title && !addPossible);
            }

            return result;
        }

    }
}