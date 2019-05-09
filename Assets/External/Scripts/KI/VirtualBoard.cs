using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    [Serializable]
    public class VirtualBoard : IBoard<VirtualBoard>
    {
        public Player opponent;

        public void SayKoikoi(bool koikoi)
        {
            isFinal = !koikoi;
            LastMove.Koikoi = koikoi;
        }
        public VirtualBoard(Spielfeld root) : base(root)
        {
            opponent = new Player(root.players[0]);
        }

        protected VirtualBoard(VirtualBoard board) : base(board)
        {
            opponent = new Player(board.opponent);
        }

        /// <summary>
        /// KI-Konstruktor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="move"></param>
        /// <param name="Turn"></param>
        public override VirtualBoard ApplyMove(Coords boardCoords, Move move)
        {
            VirtualBoard board = new VirtualBoard(this);
            //WICHTIG! Einsammeln bei Kartenzug!
            board.parentCoords = boardCoords;

            Card handSelection = board.active.Hand.Find(x => x.Title == move.HandSelection);
            List<Card> handMatches = new List<Card>();

            Card deckSelection = Deck.Find(x => x.Title == move.DeckSelection);
            List<Card> deckMatches = new List<Card>();

            List<Card> collectedCards = new List<Card>();

            for (int i = Field.Count - 1; i >= 0; i--)
            {
                if (move.HandFieldSelection.Length > 0)
                {
                    if (Field[i].Title == move.HandFieldSelection)
                    {
                        handMatches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == handSelection.Monat)
                {
                    handMatches.Add(Field[i]);
                    Field.RemoveAt(i);
                    continue;
                }

                if (move.DeckFieldSelection.Length > 0)
                {
                    if (Field[i].Title == move.DeckFieldSelection)
                    {
                        deckMatches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == deckSelection.Monat)
                {
                    deckMatches.Add(Field[i]);
                    Field.RemoveAt(i);
                    continue;
                }
            }

            active.Hand.Remove(handSelection);
            if (handMatches.Count > 0)
            {
                handMatches.Add(handSelection);
                active.CollectedCards.AddRange(handMatches);
                collectedCards.AddRange(handMatches);
            }
            else
            {
                Field.Add(handSelection);
            }

            Deck.Remove(deckSelection);
            if (deckMatches.Count > 0)
            {
                deckMatches.Add(deckSelection);
                active.CollectedCards.AddRange(deckMatches);
                collectedCards.AddRange(deckMatches);
            }
            else
            {
                Field.Add(deckSelection);
            }

            HasNewYaku = Yaku.GetNewYakus(active, collectedCards).Count > 0;

            LastMove = move;
            LastMove.HadYaku = HasNewYaku;
            return null;
        }
    }
}