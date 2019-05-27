using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public partial class ConsultingMoveBuilder
    {
        private void Awake()
        {
            HandSelectionParent = MainSceneVariables.consultingTransforms.HandSelection;
            HandFieldSelectionParent = MainSceneVariables.consultingTransforms.HandFieldSelection;
            DeckSelectionParent = MainSceneVariables.consultingTransforms.DeckSelection;
            DeckFieldSelectionParent = MainSceneVariables.consultingTransforms.DeckFieldSelection;
            Confirm = MainSceneVariables.consultingTransforms.MoveConfirm;
        }

        public void SetMoveOptions(Spielfeld board)
        {
            foreach (Card card in board.players[Turn ? 0 : 1].Hand)
            {
                GameObject cardObject = CreateCard(HandSelectionParent, card);
            }
            foreach (Card card in board.Deck)
            {
                GameObject cardObject = CreateCard(DeckSelectionParent, card);
            }
        }

        public void SetUninformedMoveOptions(Spielfeld board)
        {
            List<Card> handCards;
            List<Card> UnknownCards = Global.allCards
                .Except(board.players[0].Hand)
                .Except(board.players[0].CollectedCards)
                .Except(board.Field)
                .Except(board.players[1].CollectedCards)
                .ToList();
            if (Turn) handCards = board.players[0].Hand;
            else handCards = UnknownCards;

            foreach (Card card in handCards)
            {
                GameObject cardObject = CreateCard(HandSelectionParent, card);
            }
            foreach (Card card in UnknownCards)
            {
                GameObject cardObject = CreateCard(DeckSelectionParent, card);
            }
        }

        public void SetupMoveBuilder(Spielfeld board, bool turn)
        {
            Board = board;
            Turn = turn;
            action = new PlayerAction();
            action.Init(Board);

            board.players.Reverse();
            IArtificialIntelligence computer = (IArtificialIntelligence)KI.Init(Settings.AiMode, "Computer");
            aiRecommendation = PlayerAction.FromMove(computer.MakeTurn(board), board);
            Debug.Log(aiRecommendation.ToString());

            ResetUI();

            if (Settings.AiMode == Settings.AIMode.Omniscient)
                SetMoveOptions(board);
            else
                SetUninformedMoveOptions(board);
        }

        private void ResetUI()
        {
            LastSelections = new Dictionary<Transform, RawImage>();
            LastSelections.Add(HandSelectionParent, null);
            LastSelections.Add(HandFieldSelectionParent, null);
            LastSelections.Add(DeckSelectionParent, null);
            LastSelections.Add(DeckFieldSelectionParent, null);

            Confirm.onClick.RemoveAllListeners();
            Confirm.onClick.AddListener(() => ConfirmMove());

            foreach (Transform child in HandSelectionParent)
                Destroy(child.gameObject);
            foreach (Transform child in HandFieldSelectionParent)
                Destroy(child.gameObject);
            foreach (Transform child in DeckSelectionParent)
                Destroy(child.gameObject);
            foreach (Transform child in DeckFieldSelectionParent)
                Destroy(child.gameObject);
        }

        private GameObject CreateCard(Transform parent, Card card)
        {
            GameObject cardObject = new GameObject();
            cardObject.transform.SetParent(parent, false);

            RawImage cardImage = cardObject.AddComponent<RawImage>();
            cardImage.texture = card.Image.mainTexture;
            cardImage.color = new Color(.5f, .5f, .5f);

            Button cardButton = cardObject.AddComponent<Button>();
            if (card == aiRecommendation.HandSelection
                || card == aiRecommendation.HandFieldSelection
                || card == aiRecommendation.DeckSelection
                || card == aiRecommendation.DeckFieldSelection)
            {
                cardButton.colors = new ColorBlock() { normalColor = Color.yellow, colorMultiplier = 1, highlightedColor = Color.yellow, pressedColor = Color.yellow };
            }
            cardButton.onClick.AddListener(() =>
            {
                bool isActive = cardImage.color == Color.white;
                if (!isActive)
                {
                    cardImage.color = Color.white;
                    if (LastSelections[parent])
                        LastSelections[parent].color = new Color(.5f, .5f, .5f);
                    if (cardImage)
                    {
                        LastSelections[parent] = cardImage;
                        OnSelectionChanged(parent, card);
                    }
                }
            });

            return cardObject;
        }

        private void InitCard(Card target, Card source)
        {
            target.ID = source.ID;
            target.Image = source.Image;
            target.Monat = source.Monat;
            target.Title = source.Title;
            target.Typ = source.Typ;
        }
    }
}
