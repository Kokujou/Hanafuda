
using Hanafuda.Base.Interfaces;
using Hanafuda.Extensions;
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

        private IArtificialIntelligence activeAI;

        private bool HadYaku;
        private bool Koikoi;

        private Dictionary<Transform, RawImage> LastSelections;

        private void ApplyMove(PlayerAction move)
        {
            Consulting.MarkAreas(false, !Turn);
            gameObject.SetActive(false);
            if(Settings.AiMode.IsOmniscient())
            {
                Board.Deck[0].SetObject(action.DeckSelection.GetObject());
                Board.Deck[0] = action.DeckSelection;
                GameObject deckSelection = Board.Deck[0].GetObject();
                deckSelection.name = action.DeckSelection.Title;
                deckSelection.GetComponentsInChildren<MeshRenderer>()[0].material = action.DeckSelection.GetImage();
                action.DeckSelection.SetObject( deckSelection);

                if (!action.HandSelection.GetObject())
                {
                    Board.Players[1].Hand[0] = action.HandSelection;
                    GameObject handSelection = Board.Players[1].Hand[0].GetObject();
                    handSelection.name = action.HandSelection.Title;
                    handSelection.GetComponentsInChildren<MeshRenderer>()[0].material = action.HandSelection.GetImage();
                    action.HandSelection.SetObject(handSelection);
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
            if (action.HandSelection == null) return false;
            if (Board.Field.FindAll(x => x.Month == action.HandSelection.Month).Count == 2
                && action.HandFieldSelection == null) return false;
            if (action.DeckSelection == null) return false;
            if (Board.Field.FindAll(x => x.Month == action.DeckSelection.Month).Count == 2
                && action.DeckFieldSelection == null) return false;
            if (action.HandFieldSelection == action.DeckFieldSelection
                && action.HandFieldSelection != null) return false;
            return true;
        }

        private List<ICard> GetCollectedCards(PlayerAction action)
        {
            List<ICard> toCollect = new List<ICard>();
            List<ICard> handFieldMatches = Board.Field.FindAll(x => x.Month == action.HandSelection.Month);
            if (handFieldMatches.Count > 0)
            {
                toCollect.Add(action.HandSelection);
                if (action.HandFieldSelection != null)
                    toCollect.Add(action.HandFieldSelection);
                else
                    toCollect.AddRange(handFieldMatches);
            }
            List<ICard> deckFieldMatches = Board.Field.FindAll(x => x.Month == action.DeckSelection.Month);
            if (deckFieldMatches.Count > 0)
            {
                toCollect.Add(action.DeckSelection);
                if (action.DeckFieldSelection != null)
                    toCollect.Add(action.DeckFieldSelection);
                else
                    toCollect.AddRange(deckFieldMatches);
            }
            return toCollect;
        }
    }
}