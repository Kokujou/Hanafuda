using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    [Serializable]
    public class OmniscientBoard : IBoard<OmniscientBoard>
    {
        public Player player;
        public List<Card> Deck;

        protected override OmniscientBoard Clone() => new OmniscientBoard(this);

        public OmniscientBoard(Spielfeld root) : base(root)
        {
            player = new Player(root.players[Settings.PlayerID]);
            Deck = new List<Card>(root.Deck);
        }

        protected OmniscientBoard(OmniscientBoard board) : base(board)
        {
            player = new Player(board.player);
            Deck = new List<Card>(board.Deck);
        }

        protected override void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn)
        {
            Player activePlayer = turn ? computer : player;

            List<Card> target = fromHand ? activePlayer.Hand : Deck;

            Card selectedCard = target.Find(x => x.Title == selection);
            List<Card> matches = new List<Card>();

            //Build Matches and Remove from Field
            for (int i = Field.Count - 1; i >= 0; i--)
            {
                if (secondSelection.Length > 0)
                {
                    if (Field[i].Title == secondSelection)
                    {
                        matches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == selectedCard.Monat)
                {
                    matches.Add(Field[i]);
                    Field.RemoveAt(i);
                }
            }

            //Collect Cards or add to Field
            target.Remove(selectedCard);
            if (matches.Count > 0)
            {
                matches.Add(selectedCard);
                activePlayer.CollectedCards.AddRange(matches);
            }
            else
            {
                Field.Add(selectedCard);
            }
        }

        protected override bool CheckYaku(bool turn)
        {
            List<Card> activeCollection = turn ? computer.CollectedCards : player.CollectedCards;
            return Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), activeCollection).Count > 0;
        }
    }
}