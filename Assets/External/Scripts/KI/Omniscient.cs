using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class OmniscientAI : KI
    {
        public OmniscientAI(string name) : base(name) { }
        public override Move MakeTurn(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            Tree = new StateTree(cRoot);
            Tree.Build(1);
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            List<List<VirtualBoard>> stateTree = new List<List<VirtualBoard>>();
            for (var i = 0; i < stateTree[1].Count; i++)
            {
                stateTree[1][i].Value = RateState(stateTree[1][i]);
                if (stateTree[1][i].Value > maxValue)
                {
                    maxValue = stateTree[1][i].Value;
                    selectedMove = stateTree[1][i].LastMove;
                }
            }
            return selectedMove;
        }
        public override float RateState(VirtualBoard State)
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
            float result = 0;

            if (State.isFinal) return 1;

            int RoundsLeft = 0;

            Player User = State.players[State.Turn ? Settings.PlayerID : 1 - Settings.PlayerID];
            Player Computer = State.players[State.Turn ? 1 - Settings.PlayerID : Settings.PlayerID];

            List<Card> UPlayableCards = User.Hand;
            for (int uDeck = 0; uDeck < User.Hand.Count; uDeck += 2)
            {
                int Add = 0;
                if (State.Turn)
                    Add = 1;
                UPlayableCards.Add(State.Deck[uDeck + Add]);
            }

            List<Card> CPlayableCards = Computer.Hand;
            for (int cDeck = 0; cDeck < Computer.Hand.Count; cDeck += 2)
            {
                int Add = 0;
                if (!State.Turn)
                    Add = 1;
                CPlayableCards.Add(State.Deck[cDeck + Add]);
            }

            SortedList<Card.Months, float[]> UCollectableMonths = new SortedList<Card.Months, float[]>();
            for (int uCard = 0; uCard < UPlayableCards.Count; uCard++)
            {
                if (!UCollectableMonths.ContainsKey(UPlayableCards[uCard].Monat))
                    UCollectableMonths.Add(UPlayableCards[uCard].Monat, new float[3]);
                else
                    UCollectableMonths[UPlayableCards[uCard].Monat][0]++;
            }

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

            oReachableCards.AddRange(State.Deck);
            oReachableCards.AddRange(((Player)State.players[0]).Hand);
            oReachableCards.AddRange(State.Field);
            oReachableCards.AddRange(((Player)State.players[0]).CollectedCards);
            pReachableCards.AddRange(State.Deck);
            pReachableCards.AddRange(((Player)State.players[1]).Hand);
            pReachableCards.AddRange(State.Field);
            pReachableCards.AddRange(((Player)State.players[1]).CollectedCards);

            pPossibleYaku.AddRange(Yaku.GetYaku(pReachableCards));

            oPossibleYaku.AddRange(Yaku.GetYaku(oReachableCards));


            return result;
        }

    }
}