using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public class ConsultingMoveBuilder : MonoBehaviour
    {
        public Transform
            HandSelectionParent,
            HandFieldSelectionParent,
            DeckSelectionParent,
            DeckFieldSelectionParent;
        public Button Confirm;

        private Spielfeld Board;
        private bool Turn;
        private PlayerAction action;

        private Dictionary<Transform, RawImage> LastSelections;

        public void SetupMoveBuilder(Spielfeld board, bool turn)
        {
            Board = board;
            Turn = turn;
            action = new PlayerAction();

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

            LastSelections = new Dictionary<Transform, RawImage>();
            LastSelections.Add(HandSelectionParent, null);
            LastSelections.Add(HandFieldSelectionParent, null);
            LastSelections.Add(DeckSelectionParent, null);
            LastSelections.Add(DeckFieldSelectionParent, null);

            foreach (Card card in board.players[turn ? 0 : 1].Hand)
            {
                GameObject cardObject = CreateCard(HandSelectionParent, card);
            }
            foreach (Card card in board.Deck)
            {
                GameObject cardObject = CreateCard(DeckSelectionParent, card);
            }
        }

        private GameObject CreateCard(Transform parent, Card card)
        {
            GameObject cardObject = new GameObject();
            cardObject.transform.SetParent(parent, false);

            RawImage cardImage = cardObject.AddComponent<RawImage>();
            cardImage.texture = card.Image.mainTexture;
            cardImage.color = new Color(.5f, .5f, .5f);

            Button cardButton = cardObject.AddComponent<Button>();
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

        private void OnSelectionChanged(Transform key, Card selected)
        {
            Transform parent = null;
            if (key == HandSelectionParent)
            {
                parent = HandFieldSelectionParent;
                action.HandSelection = selected;
            }
            else if (key == DeckSelectionParent)
            {
                parent = DeckFieldSelectionParent;
                action.DeckSelection = selected;
            }
            else if (key == HandFieldSelectionParent)
                action.HandFieldSelection = selected;
            else if (key == DeckFieldSelectionParent)
                action.DeckFieldSelection = selected;
            if (parent)
            {
                foreach (Transform child in parent)
                    Destroy(child.gameObject);
                List<Card> matches = Board.Field.FindAll(x => x.Monat == selected.Monat);
                foreach (Card match in matches)
                {
                    RawImage cardImage = CreateCard(parent, match).GetComponent<RawImage>();
                    cardImage.color = matches.Count == 2 ?
                        new Color(.5f, .5f, .5f) : Color.white;
                }
            }
        }

        private void ConfirmMove()
        {
            Board.Turn = !Board.Turn;
            Board.MarkAreas(false);
            gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}