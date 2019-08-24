using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public class Consulting : MonoBehaviour
    {
        public CardDropContainer
            PlayerHand,
            Field,
            PlayerCollection,
            OpponentCollection,
            DeckSelection;

        public RectTransform
            HandSelectionContainer,
            DeckSelectionContainer;

        private Move currentMove;
        private IArtificialIntelligence player;
        private IHanafudaBoard board;

        public void GetRecommendation()
        {
            board = BuildUninformedBoard();
            player = (IArtificialIntelligence)board.Players[0];

            currentMove = player.MakeTurn(board, 1);

            ViewRecommendation(HandSelectionContainer, currentMove.HandSelection, currentMove.HandFieldSelection);

        }

        private void RequestDeckSelection()
        {
            var deckSelection = DeckSelection.Inventory[0];
            currentMove.DeckSelection = deckSelection.Title;

            var deckMove = player.RequestDeckSelection(board, currentMove, 1);
            ViewRecommendation(DeckSelectionContainer, currentMove.DeckSelection, currentMove.DeckFieldSelection);
        }

        private void ViewRecommendation(Transform parent, string firstSelection, string secondSelection)
        {
            var handSelection = Global.allCards.Find(x => x.Title == firstSelection);
            var cardObject = BuildCardObject(handSelection);
            cardObject.transform.SetParent(parent, true);

            if (secondSelection.Length <= 0)
                return;

            var handFieldSelection = Global.allCards.Find(x => x.Title == secondSelection);
            cardObject = BuildCardObject(handFieldSelection);
            cardObject.transform.SetParent(parent);
        }

        private GameObject BuildCardObject(Card card)
        {
            var result = new GameObject(card.Title);

            result.AddComponent<RawImage>().texture = card.Image.mainTexture;

            return result;
        }

        private VirtualBoard BuildUninformedBoard()
        {
            var players = new List<Player>() { GetPlayer(), GetOpponent() };

            var virtualBoard = new VirtualBoard()
            {
                Field = Field.Inventory,
                Players = players
            };

            return virtualBoard;
        }

        private Player GetPlayer()
        {
            var player = KI.Init(Settings.AiMode, "Player");

            player.Hand = PlayerHand.Inventory;
            player.CollectedCards = PlayerCollection.Inventory;

            return player;
        }

        private Player GetOpponent()
        {
            var opponent = new Player("Opponent");

            opponent.CollectedCards = OpponentCollection.Inventory;

            return opponent;
        }
    }
}