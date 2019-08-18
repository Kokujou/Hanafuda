using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class SearchingBoard : IBoard<SearchingBoard>
    {
        public List<Card> playerHand;
        public List<Card> computerHand;
        public List<Card> computerCollection;
        public readonly Dictionary<int, Card> Deck;
        public List<int> CardsCollected;
        public int TurnID;

        public SearchingBoard() { }

        public SearchingBoard(SearchingBoard target)
        {
            Field = new List<Card>(target.Field);
            Deck = new Dictionary<int, Card>(target.Deck);
            playerHand = new List<Card>(target.playerHand);
            computerHand = new List<Card>(target.computerHand);
            computerCollection = new List<Card>(target.computerCollection);
            CardsCollected = new List<int>(target.CardsCollected);
            Root = target.Root;
            TurnID = target.TurnID;
        }

        public SearchingBoard(IHanafudaBoard root, int playerID)
        {
            Turn = root.Turn;
            Field = new List<Card>(root.Field);
            LastMove = null;
            computer = root.Players[1 - playerID];
            Value = 0f;
            isFinal = false;
            playerHand = new List<Card>(root.Players[playerID].Hand);
            computerHand = new List<Card>(computer.Hand);
            computerCollection = new List<Card>(computer.CollectedCards);
            Deck = root.Deck.ToDictionary(24);
            CardsCollected = new List<int>(8);
            Root = -1;
            TurnID = 0;
            Global.Log(string.Join(";", computerHand));
        }

        protected override bool CheckYaku(bool turn)
        {
            List<Card> activeCollection = computerCollection;
            return Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), activeCollection).Count > 0;
        }

        public override SearchingBoard Clone()
            => new SearchingBoard(this);

        protected override void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn)
        {
            List<Card> target = fromHand ? computerHand : Deck.Values.ToList();

            Card selectedCard = target.Find(x => x.Title == selection);
            if (selectedCard == null)
                Debug.Log($"{selection} is not present in List from { (turn ? "computer" : "player")}");
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
                computerCollection.AddRange(matches);
            }
            else
            {
                Field.Add(selectedCard);
            }
        }
    }
}
