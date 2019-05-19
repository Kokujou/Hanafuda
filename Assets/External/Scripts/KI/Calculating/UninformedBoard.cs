using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public partial class CalculatingAI
    {
        [Serializable]
        public class UninformedBoard : IBoard<UninformedBoard>
        {
            public Dictionary<int, int> CollectedYaku;

            /// <summary>
            /// Unbekannte Karten mit Wahrscheinlichkeit, zur Hand des Gegners zu gehören.
            /// </summary>
            public Dictionary<Card, float> UnknownCards;

            public List<Card> OpponentCollection;

            public UninformedBoard(Spielfeld root) : base(root)
            {
                CollectedYaku = root.players[Settings.PlayerID].CollectedYaku;
                OpponentCollection = root.players[Settings.PlayerID].CollectedCards;
                UnknownCards = Global.allCards
                    .Except(OpponentCollection)
                    .Except(computer.Hand)
                    .Except(computer.CollectedCards)
                    .Except(Deck)
                    .Except(Field)
                    .ToDictionary(x => x, x => 0f);
            }

            protected UninformedBoard(UninformedBoard target) : base(target)
            {
                CollectedYaku = new Dictionary<int, int>(target.CollectedYaku);
                UnknownCards = new Dictionary<Card, float>(target.UnknownCards);
                OpponentCollection = new List<Card>(target.OpponentCollection);
            }

            /// <summary>
            /// Uninformierter Spielzug der KI, Hand bekannt, Deckzug unbekannt
            /// </summary>
            /// <param name="boardCoords">Eltern-Koordinaten des neuen Spielfelds</param>
            /// <param name="move">getätigter Spielzug</param>
            /// <param name="turn">Immer wahr in dieser Überladung</param>
            /// <returns></returns>
            public override UninformedBoard ApplyMove(Coords boardCoords, Move move, bool turn)
            {
                turn = true;

                UninformedBoard board = new UninformedBoard(this);
                board.parentCoords = boardCoords;

                List<Card> activeCollection = turn ? board.computer.CollectedCards : board.OpponentCollection;
                Dictionary<Card, float> activeHand = turn ? board.computer.Hand.ToDictionary(x => x, x => 1f) : board.UnknownCards;

                Card handSelection = activeHand.First(x => x.Key.Title == move.HandSelection).Key;
                List<Card> handMatches = new List<Card>();

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
                }

                activeHand.Remove(handSelection);
                if (handMatches.Count > 0)
                {
                    handMatches.Add(handSelection);
                    activeCollection.AddRange(handMatches);
                    collectedCards.AddRange(handMatches);
                }
                else
                {
                    board.Field.Add(handSelection);
                }

                if (turn)
                    board.HasNewYaku = Yaku.GetNewYakus(board.computer, collectedCards).Count > 0;
                else
                    board.HasNewYaku = Yaku.GetNewYakus(board.CollectedYaku, collectedCards).Count > 0;


                board.LastMove = move;
                board.LastMove.HadYaku = HasNewYaku;
                return board;
            }

            /// <summary>
            /// Spielzug des Spielers aus Sicht der KI, unbekannte Hand
            /// </summary>
            /// <param name="boardCoords">Eltern-Koordinaten des erstellen Spielfeldes</param>
            /// <param name="move">getätigter Spielzug</param>
            /// <param name="prob">Ausgabe der Wahrscheinlichkeit, dass die ausgewählte Karte auch zur Hand gehört</param>
            /// <returns></returns>
            public UninformedBoard ApplyMove(Coords boardCoords, Move move, out float prob)
            {
                prob = UnknownCards.First(x => x.Key.Title == move.HandSelection).Value;
                return ApplyMove(boardCoords, move, false);
            }
        }
    }
}