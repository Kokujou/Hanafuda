using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hanafuda
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ConsultingSetup : MonoBehaviour
    {
        public enum Target
        {
            PlayerHand,
            OpponentHand,
            Field,
            PlayerCollection,
            OpponentCollection
        }

        public Target SetupTarget;

        private GameObject Builder;
        private ConsultingMoveBuilder MoveBuilder;
        private Transform Content;

        private List<Card> target;
        private static Spielfeld Board;

        private void Start()
        {
            Board = MainSceneVariables.boardTransforms.Main;
            Content = MainSceneVariables.consultingTransforms.SetupContent;
            MoveBuilder = MainSceneVariables.consultingTransforms.MoveBuilder;
            Builder = MainSceneVariables.consultingTransforms.ConsultingBuilder;
        }

        public static void ValidateBoard()
        {
            bool totalResult = true;
            Spielfeld board = MainSceneVariables.boardTransforms.Main;

            for (int playerID = 0; playerID < board.players.Count; playerID++)
            {
                bool tempResult = ValidateCollection(board.players[playerID]);
                if (playerID == Settings.PlayerID)
                    Consulting.ConfirmArea(Target.PlayerCollection, tempResult ?
                        Consulting.BoardValidity.Valid : Consulting.BoardValidity.Invalid);
                else
                    Consulting.ConfirmArea(Target.OpponentCollection, tempResult ?
                        Consulting.BoardValidity.Valid : Consulting.BoardValidity.Invalid);
                totalResult &= tempResult;
            }


            Consulting.BoardValidity hand2Validity;
            int diff = board.players[0].Hand.Count - board.players[1].Hand.Count;
            bool hand2Valid = true;
            if (Settings.AiMode == Settings.AIMode.Omniscient)
            {
                hand2Valid = (diff == 0 || diff == 1) && board.players[0].Hand.Count <= 8;
                if (!hand2Valid) hand2Validity = Consulting.BoardValidity.Invalid;
                else if (board.players[1 - Settings.PlayerID].Hand.Count == 0)
                    hand2Validity = Consulting.BoardValidity.Semivalid;
                else hand2Validity = Consulting.BoardValidity.Valid;
                Consulting.ConfirmArea(Target.OpponentHand, hand2Validity);
            }
            bool hand1Valid = hand2Valid && board.players[Settings.PlayerID].Hand.Count > 0;
            if (!hand1Valid || !hand2Valid)
                totalResult = false;
            Consulting.ConfirmArea(Target.PlayerHand, hand1Valid ?
                Consulting.BoardValidity.Valid : Consulting.BoardValidity.Invalid);

            Consulting.ConfirmArea(Target.Field, board.Field.Count == 0 ?
                Consulting.BoardValidity.Semivalid : Consulting.BoardValidity.Valid);

            if (board.Deck.Count < 8)
                totalResult = false;

            if (totalResult)
            {
                Global.MovingCards++;
                MessageBox messageBox = Instantiate(Global.prefabCollection.UIMessageBox).GetComponentInChildren<MessageBox>();
                messageBox.Setup("Gültiges Spielfeld",
                    "Das Spielfeld ist gültig. Möchten Sie damit das Spiel laden?",
                    new KeyValuePair<string, Action>("Ja", () => { Consulting.LoadGame(); Global.MovingCards--; }),
                    new KeyValuePair<string, Action>("Nein", () => { Global.MovingCards--; }));
            }
        }

        private static bool ValidateCollection(Player player)
        {
            bool CollectionIsValid = true;
            int[] months = new int[12];
            foreach (Card card in player.CollectedCards)
                months[(int)card.Monat]++;
            foreach (int value in months)
            {
                if (value % 2 == 1)
                {
                    CollectionIsValid = false;
                    break;
                }
            }
            return CollectionIsValid;
        }

        public void SetupMove()
        {
            if (MoveBuilder.gameObject.activeInHierarchy) return;
            MoveBuilder.gameObject.SetActive(true);
            if (SetupTarget == Target.PlayerHand)
                MoveBuilder.SetupMoveBuilder(Board, true);
            else if (SetupTarget == Target.OpponentHand)
                MoveBuilder.SetupMoveBuilder(Board, false);
        }

        public void SetupArea()
        {
            if (Builder.activeInHierarchy) return;
            Builder.SetActive(true);
            target = new List<Card>();
            switch (SetupTarget)
            {
                case Target.PlayerHand:
                    target = Board.players[Settings.PlayerID].Hand;
                    break;
                case Target.OpponentHand:
                    target = Board.players[1 - Settings.PlayerID].Hand;
                    break;
                case Target.Field:
                    target = Board.Field;
                    break;
                case Target.PlayerCollection:
                    target = Board.players[Settings.PlayerID].CollectedCards;
                    break;
                case Target.OpponentCollection:
                    target = Board.players[1 - Settings.PlayerID].CollectedCards;
                    break;
            }

            for (int cardID = 0; cardID < target.Count + Board.Deck.Count; cardID++)
            {
                Card card;
                if (cardID < target.Count)
                    card = target[cardID];
                else
                    card = Board.Deck[cardID - target.Count];
                GameObject cardObject = new GameObject("Card");
                cardObject.transform.SetParent(Content, false);
                RawImage cardImage = cardObject.AddComponent<RawImage>();
                cardImage.texture = card.Image.mainTexture;
                if (cardID < target.Count)
                    cardImage.color = Color.white;
                else
                    cardImage.color = new Color(.5f, .5f, .5f);
                Button cardButton = cardObject.AddComponent<Button>();
                cardButton.onClick.AddListener(() =>
                {
                    bool isActive = cardImage.color == Color.white;
                    if (isActive)
                    {
                        cardImage.color = new Color(.5f, .5f, .5f);
                        target.Remove(card);
                        Board.Deck.Add(card);
                    }
                    else
                    {
                        cardImage.color = Color.white;
                        target.Add(card);
                        Board.Deck.Remove(card);
                    }
                });

            }
        }
    }
}
