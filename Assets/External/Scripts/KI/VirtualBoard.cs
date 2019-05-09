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
        public override VirtualBoard ApplyMove(Coords boardCoords, Move move, bool turn)
        {
            VirtualBoard board = new VirtualBoard(this);
            //WICHTIG! Einsammeln bei Kartenzug!
            board.parentCoords = boardCoords;

            Player activePlayer = turn ? board.active : board.opponent;

            Card handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);
            List<Card> handMatches = new List<Card>();

            Card deckSelection = board.Deck.Find(x => x.Title == move.DeckSelection);
            List<Card> deckMatches = new List<Card>();

            List<Card> collectedCards = new List<Card>();

            for (int i = board.Field.Count - 1; i >= 0; i--)
            {
                if (move.HandFieldSelection.Length > 0)
                {
                    if (board.Field[i].Title == move.HandFieldSelection)
                    {
                        handMatches.Add(board.Field[i]);
                        board.Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (board.Field[i].Monat == handSelection.Monat)
                {
                    handMatches.Add(board.Field[i]);
                    board.Field.RemoveAt(i);
                    continue;
                }

                if (move.DeckFieldSelection.Length > 0)
                {
                    if (board.Field[i].Title == move.DeckFieldSelection)
                    {
                        deckMatches.Add(board.Field[i]);
                        board.Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (board.Field[i].Monat == deckSelection.Monat)
                {
                    deckMatches.Add(board.Field[i]);
                    board.Field.RemoveAt(i);
                    continue;
                }
            }

            activePlayer.Hand.Remove(handSelection);
            if (handMatches.Count > 0)
            {
                handMatches.Add(handSelection);
                activePlayer.CollectedCards.AddRange(handMatches);
                collectedCards.AddRange(handMatches);
            }
            else
            {
                board.Field.Add(handSelection);
            }

            board.Deck.Remove(deckSelection);
            if (deckMatches.Count > 0)
            {
                deckMatches.Add(deckSelection);
                activePlayer.CollectedCards.AddRange(deckMatches);
                collectedCards.AddRange(deckMatches);
            }
            else
            {
                board.Field.Add(deckSelection);
            }

            board.HasNewYaku = Yaku.GetNewYakus(activePlayer, collectedCards).Count > 0;

            board.LastMove = move;
            board.LastMove.HadYaku = HasNewYaku;
            return board;
        }
    }
}