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
            Dictionary<Card.Months, int> playerHandMonths = Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => 0);

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

                if (opponent)
                    return matches;

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
                var result = new List<SearchingBoard>(8);
                var disposableResults = new List<SearchingBoard>(8);

                if (parent.isFinal)
                    return result;

                foreach (Card card in parent.computerHand)
                {
                    SearchingBoard child = parent.Clone();
                    child.TurnID = parent.TurnID + 1;
                    List<Card> handMatches = TryCollect(child.Field, parent.TurnID, card);
                    int collectedCards = 0;
                    int oldFieldCount = child.Field.Count;
                    if (handMatches.Count > 0)
                    {
                        child.computerCollection.Add(card);
                        child.computerCollection.AddRange(handMatches);
                        collectedCards += 1 + handMatches.Count;
                        foreach (Card match in handMatches)
                            child.Field.Remove(match);
                    }
                    else
                    {
                        child.Field.Add(card);
                    }

                    int deckID = (parent.TurnID) * 2;
                    Card deckCard = child.Deck[deckID];
                    List<Card> deckMatches = TryCollect(child.Field, parent.TurnID, deckCard);
                    if (deckMatches.Count > 0)
                    {
                        child.computerCollection.Add(deckCard);
                        child.computerCollection.AddRange(deckMatches);
                        collectedCards += 1 + deckMatches.Count;
                        foreach (Card match in deckMatches)
                            child.Field.Remove(match);
                    }
                    else
                    {
                        child.Field.Add(deckCard);
                    }

                    int oppDeckID = (parent.TurnID) * 2 + 1;
                    Card oppDeckCard = child.Deck[deckID];
                    List<Card> oppDeckMatches = TryCollect(child.Field, parent.TurnID, oppDeckCard, true);
                    if (oppDeckMatches.Count == 0)
                    {
                        child.Field.Add(oppDeckCard);
                    }

                    List<Card> newHand = new List<Card>(8);
                    foreach (Card newCard in parent.computerHand)
                        if (newCard != card)
                            newHand.Add(newCard);
                    child.computerHand = newHand;
                    child.LastMove = new Move();
                    child.LastMove.HandSelection = card.Title;
                    child.LastMove.DeckSelection = deckCard.Title;
                    child.CardsCollected.Add(collectedCards);

                    if (child.computerHand.Count == 0)
                    {
                        child.isFinal = true;
                    }

                    if (child.Field.Count > oldFieldCount)
                        disposableResults.Add(child);
                    else
                        result.Add(child);
                }
                if (result.Count == 0)
                    result.AddRange(disposableResults);

                param = null;
                return result;
            }
        }
    }
}