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
        public void ResetBuilder()
        {
            SetupMoveBuilder(Board, Turn);
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
                if (!Settings.AiMode.IsOmniscient() && matches.Count == 2 && key == DeckSelectionParent)
                {
                    Board.Players.Reverse();
                    aiRecommendation.DeckSelection = action.DeckSelection;
                    aiRecommendation = PlayerAction.FromMove(activeAI.RequestDeckSelection(Board, aiRecommendation), Board);
                    Board.Players.Reverse();
                }
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
            List<Yaku> newYakus = Yaku.GetNewYakus(Board.Players[action.PlayerID], GetCollectedCards(action));
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


    }
}
