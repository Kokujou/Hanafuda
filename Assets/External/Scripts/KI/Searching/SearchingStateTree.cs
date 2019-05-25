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
            private List<Card> TryCollect(List<Card> Field, int turnID, Card card, bool opponent = false)
            {
                List<Card> emptyReturn = new List<Card>(0);
                List<Card> matches = new List<Card>(4);

                foreach (Card fieldCard in Field)
                    if (fieldCard.Monat == card.Monat)
                        matches.Add(fieldCard);

                if (opponent) return matches;

                if (matches.Count == 0)
                    return matches;

                if (playerHandMonths[card.Monat] > 0 && matches.Count != 2)
                    return emptyReturn;

                if (turnID > playerDeck[card.Monat])
                    return emptyReturn;

                return matches;
            }

            protected override object BuildChildNodes(object param)
            {
                SearchingBoard parent = (SearchingBoard)param;
                List<SearchingBoard> result = new List<SearchingBoard>(8);
                int turnID = 8 - parent.computerHand.Count;
                foreach (Card card in parent.computerHand)
                {
                    SearchingBoard child = new SearchingBoard(parent);
                    List<Card> newCollection = new List<Card>( parent.computerCollection);
                    List<Card> newField = new List<Card>(16);
                    newField.AddRange(parent.Field);
                    List<Card> matches = TryCollect(newField, turnID, card);
                    if (matches.Count > 0)
                    {
                        newCollection.Add(card);
                        newCollection.AddRange(matches);
                        child.CardsCollected += 1 + matches.Count;
                        foreach (Card match in matches)
                            newField.Remove(match);
                    }
                    else
                    {
                        newField.Add(card);
                    }

                    int deckID = (turnID) * 2;
                    Card deckCard = parent.Deck[deckID];
                    List<Card> deckMatches = TryCollect(newField, turnID, deckCard);
                    if (deckMatches.Count > 0)
                    {
                        newCollection.Add(deckCard);
                        newCollection.AddRange(deckMatches);
                        child.CardsCollected += 1 + deckMatches.Count;
                        foreach (Card match in deckMatches)
                            newField.Remove(match);
                    }
                    else
                    {
                        newField.Add(deckCard);
                    }

                    int oppDeckID = (turnID) * 2 + 1;
                    Card oppDeckCard = parent.Deck[deckID];
                    List<Card> oppDeckMatches = TryCollect(newField, turnID, oppDeckCard, true);
                    if (oppDeckMatches.Count == 0)
                    {
                        newField.Add(oppDeckCard);
                    }

                    List<Card> newHand = new List<Card>(8);
                    foreach (Card newCard in parent.computerHand)
                        if (newCard != card)
                            newHand.Add(newCard);
                    child.computerHand = newHand;
                    child.computerCollection = newCollection;
                    child.Field = newField;
                    child.LastMove = new Move();
                    child.LastMove.HandSelection = card.Title;
                    child.LastMove.DeckSelection = deckCard.Title;
                    result.Add(child);
                }
                return result;
            }

        }
    }
}