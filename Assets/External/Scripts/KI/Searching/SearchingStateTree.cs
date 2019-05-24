using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public partial class SearchingAI
    {
        public class SearchingStateTree : IStateTree<SearchingBoard>
        {
            Dictionary<Card.Months, int> computerDeck = Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => 9);
            Dictionary<Card.Months, int> playerDeck = Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => 9);
            Dictionary<Card.Months, int> playerHandMonths = Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => 9);

            public SearchingStateTree(SearchingBoard root, List<List<SearchingBoard>> tree = null) : base(root, tree)
            {
                foreach (Card card in root.playerHand)
                    playerHandMonths[card.Monat]++;
                for (int cardID = 0; cardID < root.computerHand.Count; cardID++)
                {
                    int deckID = cardID * 2;
                    if (computerDeck[root.Deck[deckID].Monat] > 8)
                        computerDeck[root.Deck[deckID].Monat] = cardID;
                }
                for (int cardID = 0; cardID < root.playerHand.Count; cardID++)
                {
                    int deckID = cardID * 2 + 1;
                    if (playerDeck[root.Deck[deckID].Monat] > 8)
                        playerDeck[root.Deck[deckID].Monat] = cardID;
                }
            }

            /// <summary>
            /// Get possible matches for card assuming turn is always true
            /// </summary>
            /// <param name="board"></param>
            /// <param name="card"></param>
            /// <returns></returns>
            private List<Card> TryCollect(SearchingBoard board, Card card)
            {
                List<Card> emptyReturn = new List<Card>();
                List<Card> matches = new List<Card>();

                for (int i = 0; i < board.Field.Count; i++)
                    if (board.Field[i].Monat == card.Monat)
                        matches.Add(board.Field[i]);

                if (matches.Count == 0)
                    return matches;

                if (playerHandMonths[card.Monat] > 0 && matches.Count != 2)
                    return emptyReturn;

                int turnID = 8 - board.computerHand.Count;
                if (turnID > playerDeck[card.Monat])
                    return emptyReturn;

                return matches;

            }

            protected override object BuildChildNodes(object param)
            {
                bool fieldCopied = false;
                SearchingBoard parent = (SearchingBoard)param;
                List<SearchingBoard> result = new List<SearchingBoard>();

                List<Card> targetHand = parent.Turn ? parent.computerHand : parent.playerHand;
                List<Card> targetCollection = parent.Turn ? parent.computerCollection : parent.playerCollection;
                for (int cardID = 0; cardID < targetHand.Count; cardID++)
                {
                    Card card = targetHand[cardID];
                    SearchingBoard child = new SearchingBoard(parent);
                    List<Card> matches = TryCollect(child, card);
                    if (matches.Count > 0)
                    {
                        List<Card> newCollection = new List<Card>(targetCollection);
                        newCollection.Add(card);
                        newCollection.AddRange(matches);
                        if (parent.Turn)
                            child.computerCollection = newCollection;
                        else
                            child.playerCollection = newCollection;
                    }
                    else
                    {
                        fieldCopied = true;
                        child.Field = new List<Card>();
                        child.Field.AddRange(parent.Field);
                        child.Field.Add(card);
                    }

                    int deckID = (8 - (targetHand.Count)) * 2 + (Root.Turn ? 0 : 1);
                    Card deckCard = parent.Deck[deckID];
                    List<Card> deckMatches = TryCollect(child, deckCard);
                    if (deckMatches.Count > 0)
                    {
                        List<Card> newCollection = new List<Card>(targetCollection);
                        newCollection.Add(deckCard);
                        newCollection.AddRange(deckMatches);
                        if (parent.Turn)
                            child.computerCollection = newCollection;
                        else
                            child.playerCollection = newCollection;
                    }
                    else
                    {
                        if (!fieldCopied)
                        {
                            fieldCopied = true;
                            child.Field = new List<Card>();
                            child.Field.AddRange(parent.Field);
                        }
                        child.Field.Add(deckCard);
                    }

                    int oppDeckID = (8 - (targetHand.Count)) * 2 + (Root.Turn ? 1 : 0);
                    Card oppDeckCard = parent.Deck[deckID];
                    List<Card> oppDeckMatches = TryCollect(child, oppDeckCard);
                    if (oppDeckMatches.Count == 0)
                    {
                        if (!fieldCopied)
                        {
                            fieldCopied = true;
                            child.Field = new List<Card>();
                            child.Field.AddRange(parent.Field);
                        }
                        child.Field.Add(oppDeckCard);
                    }

                    List<Card> newHand = new List<Card>(targetHand);
                    newHand.RemoveAt(cardID);
                    child.computerHand = parent.Turn ? newHand : parent.computerHand;
                    child.playerHand = parent.Turn ? parent.playerHand : newHand;

                    child.LastMove = new Move();
                    child.LastMove.HandSelection = card.Title;
                    child.LastMove.DeckSelection = deckCard.Title;
                    child.Turn = SkipOpponent ? parent.Turn : !parent.Turn;
                    result.Add(child);
                }
                return result;
            }

        }
    }
}