using System;
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

        private bool HadYaku;
        private bool Koikoi;

        private Dictionary<Transform, RawImage> LastSelections;

        public void SetupMoveBuilder(Spielfeld board, bool turn)
        {
            Board = board;
            Turn = turn;
            action = new PlayerAction();
            action.Init(Board);

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
            if (!ValidateAction(action))
            {
                MessageBox box = Instantiate(Global.prefabCollection.UIMessageBox).GetComponentInChildren<MessageBox>();
                box.Setup("Fehlerhafter Spielzug", "Der ausgewählte Spielzug ist so nicht anwendbar. Überprüfen Sie, " +
                    "ob alle Züge ausgewählt sind und ob keine identischen Matches gewählt worden.",
                    new KeyValuePair<string, Action>("OK", () => { }));
                return;
            }
            action.PlayerID = Turn ? Settings.PlayerID : 1 - Settings.PlayerID;
            List<Yaku> newYakus = Yaku.GetNewYakus(Board.players[action.PlayerID], GetCollectedCards(action));
            if (newYakus.Count > 0)
            {
                HadYaku = true;
                MessageBox box = Instantiate(Global.prefabCollection.UIMessageBox).GetComponentInChildren<MessageBox>();
                box.Setup("", "Der ausgewählte Spielzug ergibt neue Yaku.\nSagst du \"Koi Koi\"?",
                    new KeyValuePair<string, Action>("Ja", () =>
                    {
                        Koikoi = true;
                        ApplyMove(action);
                    }),
                    new KeyValuePair<string, Action>("Nein", () =>
                    {
                        Koikoi = false;
                        ApplyMove(action);
                    }),
                    new KeyValuePair<string, Action>("Abbrechen", () => { }));
            }
            else
                ApplyMove(action);
        }

        private void ApplyMove(PlayerAction move)
        {
            Board.MarkAreas(false, !Turn);
            gameObject.SetActive(false);
            Board.AnimateAction(action);
            if (HadYaku)
            {
                if(Koikoi)
                {
                    // Koikoi Actions
                }
                else
                {
                    // Game End Actions
                }
            }
        }

        private bool ValidateAction(PlayerAction action)
        {
            if (!action.HandSelection) return false;
            if (Board.Field.FindAll(x => x.Monat == action.HandSelection.Monat).Count == 2
                && !action.HandFieldSelection) return false;
            if (!action.DeckSelection) return false;
            if (Board.Field.FindAll(x => x.Monat == action.DeckSelection.Monat).Count == 2
                && !action.DeckFieldSelection) return false;
            if (action.HandFieldSelection == action.DeckFieldSelection
                && action.HandFieldSelection != null) return false;
            return true;
        }

        private List<Card> GetCollectedCards(PlayerAction action)
        {
            List<Card> toCollect = new List<Card>();
            List<Card> handFieldMatches = Board.Field.FindAll(x => x.Monat == action.HandSelection.Monat);
            if (handFieldMatches.Count > 0)
            {
                toCollect.Add(action.HandSelection);
                if (action.HandFieldSelection)
                    toCollect.Add(action.HandFieldSelection);
                else
                    toCollect.AddRange(handFieldMatches);
            }
            List<Card> deckFieldMatches = Board.Field.FindAll(x => x.Monat == action.DeckSelection.Monat);
            if (deckFieldMatches.Count > 0)
            {
                toCollect.Add(action.DeckSelection);
                if (action.DeckFieldSelection)
                    toCollect.Add(action.DeckFieldSelection);
                else
                    toCollect.AddRange(deckFieldMatches);
            }
            return toCollect;
        }
    }
}