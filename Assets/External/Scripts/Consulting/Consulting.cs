using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            DeckSelection,
            UnassignedCollection;

        public RectTransform
            HandSelectionContainer,
            HandFieldSelectionContainer,
            DeckFieldSelectionContainer;

        private Move currentMove;
        private IArtificialIntelligence player;
        private IHanafudaBoard board;

        public void GetRecommendation()
        {
            board = BuildUninformedBoard();
            player = (IArtificialIntelligence)board.Players[0];

            currentMove = player.MakeTurn(board, 1);

            if (currentMove.HandSelection.Length > 0)
                ViewCard(HandSelectionContainer, currentMove.HandSelection);
            if (currentMove.HandFieldSelection.Length > 0)
                ViewCard(HandFieldSelectionContainer, currentMove.HandFieldSelection);
        }

        public async void RequestDeckSelection()
        {
            await Task.Delay(1000);
            var deckSelection = DeckSelection.Inventory[0];
            currentMove.DeckSelection = deckSelection.Title;

            currentMove = player.RequestDeckSelection(board, currentMove, 1);
            if (currentMove.DeckFieldSelection.Length > 0)
                ViewCard(DeckFieldSelectionContainer, currentMove.DeckFieldSelection);
        }

        public void ConfirmMove()
        {
            ResetDialogue();

            if (DeckSelection.Inventory.Count != 1)
                return;

            var deckSelection = DeckSelection.GetComponentInChildren<DraggableCard>();
            deckSelection.ReassignCard(PlayerCollection);
        }

        public void AbortMove()
        {
            ResetDialogue();

            if (DeckSelection.Inventory.Count != 1)
                return;

            var deckSelection = DeckSelection.GetComponentInChildren<DraggableCard>();
            deckSelection.ReassignCard(UnassignedCollection);
        }

        private void ResetDialogue()
        {
            foreach (Transform child in HandSelectionContainer)
                Destroy(child.gameObject);
            foreach (Transform child in HandFieldSelectionContainer)
                Destroy(child.gameObject);
            foreach (Transform child in DeckFieldSelectionContainer)
                Destroy(child.gameObject);
        }

        private void ViewCard(Transform parent, string cardTitle)
        {
            var card = Global.allCards.Find(x => x.Title == cardTitle);
            var cardObject = BuildCardObject(card);
            cardObject.transform.SetParent(parent, false);
        }

        private GameObject BuildCardObject(Card card)
        {
            var result = new GameObject(card.Title);

            result.AddComponent<RawImage>().texture = card.Image.mainTexture;
            result.transform.localScale = Vector3.one;
            ((RectTransform)result.transform).sizeDelta = new Vector2(150,240); 

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