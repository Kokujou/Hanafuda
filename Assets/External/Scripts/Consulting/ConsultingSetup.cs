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
        public GameObject Builder;
        public ConsultingMoveBuilder MoveBuilder;
        public GridLayoutGroup Content;
        public bool BoardBuilded = false;

        private List<Card> target;
        private Spielfeld Board;

        private void Start()
        {
            Board = MainSceneVariables.variableCollection.Main;
            Content = Builder.GetComponentInChildren<GridLayoutGroup>();
        }

        public static void ValidateBoard()
        {
            Spielfeld Board = MainSceneVariables.variableCollection.Main;
            bool totalResult = true;
            SortedList<Target, Material> marks =
                new SortedList<Target, Material>(
                FindObjectsOfType<ConsultingSetup>().ToDictionary(
                    x => x.SetupTarget, y => y.gameObject.GetComponent<MeshRenderer>().material));

            for (int playerID = 0; playerID < Board.players.Count; playerID++)
            {
                bool tempResult = true;
                Player player = Board.players[playerID];
                int[] months = new int[12];
                foreach (Card card in player.CollectedCards)
                    months[(int)card.Monat]++;
                foreach (int value in months)
                {
                    if (value % 2 == 1)
                    {
                        totalResult = false;
                        tempResult = false;
                        break;
                    }
                }
                if (playerID == Settings.PlayerID)
                    Board.ConfirmArea(Target.PlayerCollection, tempResult ?
                        ISpielfeld.BoardValidity.Valid : ISpielfeld.BoardValidity.Invalid);
                else
                    Board.ConfirmArea(Target.OpponentCollection, tempResult ?
                        ISpielfeld.BoardValidity.Valid : ISpielfeld.BoardValidity.Invalid);
            }

            ISpielfeld.BoardValidity hand2Validity;
            int diff = Board.players[0].Hand.Count - Board.players[1].Hand.Count;
            bool hand2Valid = (diff == 0 || diff == 1) && Board.players[0].Hand.Count <= 8;
            bool hand1Valid = hand2Valid && Board.players[Settings.PlayerID].Hand.Count > 0;
            if (!hand2Valid) hand2Validity = ISpielfeld.BoardValidity.Invalid;
            else if (Board.players[1 - Settings.PlayerID].Hand.Count == 0)
                hand2Validity = ISpielfeld.BoardValidity.Semivalid;
            else hand2Validity = ISpielfeld.BoardValidity.Valid;
            if (!hand1Valid || !hand2Valid)
                totalResult = false;
            Board.ConfirmArea(Target.OpponentHand, hand2Validity);
            Board.ConfirmArea(Target.PlayerHand, hand1Valid ?
                ISpielfeld.BoardValidity.Valid : ISpielfeld.BoardValidity.Invalid);

            Board.ConfirmArea(Target.Field, Board.Field.Count == 0 ?
                ISpielfeld.BoardValidity.Semivalid : ISpielfeld.BoardValidity.Valid);

            if (Board.Deck.Count < 8)
                totalResult = false;

            if (totalResult)
            {
                Global.MovingCards++;
                MessageBox messageBox = Instantiate(Global.prefabCollection.UIMessageBox).GetComponentInChildren<MessageBox>();
                messageBox.Setup("Gültiges Spielfeld",
                    "Das Spielfeld ist gültig. Möchten Sie damit das Spiel laden?",
                    new KeyValuePair<string, Action>("Ja", () => { Board.LoadGame(); Destroy(messageBox.gameObject); }),
                    new KeyValuePair<string, Action>("Nein", () => { Destroy(messageBox.gameObject); Global.MovingCards--; }));
            }
        }

        public void SetupMove()
        {
            if (MoveBuilder.gameObject.activeInHierarchy) return;
            MoveBuilder.gameObject.SetActive(true);
            if (SetupTarget == Target.PlayerHand)
                MoveBuilder.SetupMoveBuilder(Board, true);
            else if (SetupTarget == Target.OpponentCollection)
                MoveBuilder.SetupMoveBuilder(Board,false);
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
            Global.MovingCards++;

            for (int cardID = 0; cardID < target.Count + Board.Deck.Count; cardID++)
            {
                Card card;
                if (cardID < target.Count)
                    card = target[cardID];
                else
                    card = Board.Deck[cardID - target.Count];
                GameObject cardObject = new GameObject("Card");
                cardObject.transform.SetParent(Content.transform, false);
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
