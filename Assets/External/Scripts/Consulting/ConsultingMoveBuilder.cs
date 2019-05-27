using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public partial class ConsultingMoveBuilder : MonoBehaviour
    {
        private Transform
            HandSelectionParent,
            HandFieldSelectionParent,
            DeckSelectionParent,
            DeckFieldSelectionParent;
        private Button Confirm;

        private Spielfeld Board;
        private Consulting ConsultingBoard;
        private bool Turn;
        private PlayerAction action;
        private PlayerAction aiRecommendation;

        private bool HadYaku;
        private bool Koikoi;

        private Dictionary<Transform, RawImage> LastSelections;

        private void ApplyMove(PlayerAction move)
        {
            Consulting.MarkAreas(false, !Turn);
            gameObject.SetActive(false);
            if(Settings.AiMode.IsOmniscient())
            {
                InitCard(Board.Deck[0], action.DeckSelection);
                GameObject deckSelection = Board.Deck[0].Object;
                deckSelection.name = action.DeckSelection.Title;
                deckSelection.GetComponentsInChildren<MeshRenderer>()[0].material = action.DeckSelection.Image;
                action.DeckSelection.Object = deckSelection;

                if (!action.HandSelection.Object)
                {
                    InitCard(Board.players[1].Hand[0], action.HandSelection);
                    GameObject handSelection = Board.players[1].Hand[0].Object;
                    handSelection.name = action.HandSelection.Title;
                    handSelection.GetComponentsInChildren<MeshRenderer>()[0].material = action.HandSelection.Image;
                    action.HandSelection.Object = handSelection;
                }
            }
            Board.AnimateAction(action);
            if (HadYaku)
            {
                if (Koikoi)
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