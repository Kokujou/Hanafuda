using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    [RequireComponent(typeof(Spielfeld))]
    public class Consulting : MonoBehaviour
    {
        private static Color _Valid = new Color(0, .5f, 0);
        private static Color _Invalid = new Color(.5f, 0, 0);
        private static Color _Semivalid = new Color(.5f, .5f, 0);

        private static Spielfeld Board;

        private static bool BoardBuilded = false;
        private static bool P1Oya = true;
        private static ConsultingSetup Hand1, Hand2, Field, Collection1, Collection2;

        private static List<Card> AnonymousDeck;
        private static List<Card> AnonymousOppHand;

        public enum BoardValidity
        {
            Invalid,
            Valid,
            Semivalid
        }

        private void Start()
        {
            Board = MainSceneVariables.boardTransforms.Main;
            Board.players = Settings.Players;
            Board.Deck = new List<Card>(Global.allCards);
            Board.Field = new List<Card>();
            if (Settings.Mobile)
            {
                Hand1 = MainSceneVariables.consultingTransforms.Hand1M;
                Hand2 = MainSceneVariables.consultingTransforms.Hand2M;
                Field = MainSceneVariables.consultingTransforms.MFeld;
                Collection1 = MainSceneVariables.consultingTransforms.Collection1M;
                Collection2 = MainSceneVariables.consultingTransforms.Collection2M;
            }
            else
            {
                Hand1 = MainSceneVariables.consultingTransforms.Hand1;
                Hand2 = MainSceneVariables.consultingTransforms.Hand2;
                Field = MainSceneVariables.consultingTransforms.Feld;
                Collection1 = MainSceneVariables.consultingTransforms.Collection1;
                Collection2 = MainSceneVariables.consultingTransforms.Collection2;
            }
            Button confirm = MainSceneVariables.consultingTransforms.SetupConfirm;
            confirm.onClick.AddListener(
                () =>
                {
                    ConsultingSetup.ValidateBoard();
                });
            MarkAreas();
            if (Settings.AiMode != Settings.AIMode.Omniscient)
                MainSceneVariables.consultingTransforms.OyaSelection.SetActive(true);
            MainSceneVariables.consultingTransforms.P1Toggle.onValueChanged.AddListener(x => { P1Oya = x; });
        }

        public static void MarkAreas(bool show = true, bool turn = true)
        {
            BoardBuilded = !show;
            Hand1.gameObject.SetActive(turn || show);
            Field.gameObject.SetActive(show);

            Hand2.gameObject.SetActive(!turn || (show && Settings.AiMode == Settings.AIMode.Omniscient));

            if (!Settings.Mobile)
            {
                MainSceneVariables.boardTransforms.PCCollections[0]
                    .parent.GetComponentInChildren<ConsultingSetup>(true)
                    .gameObject.SetActive(show);
                MainSceneVariables.boardTransforms.PCCollections[4]
                    .parent.GetComponentInChildren<ConsultingSetup>(true)
                    .gameObject.SetActive(show);
            }
            else
            {
                foreach (var child in FindObjectsOfType<MobileContainerArea>())
                {
                    child.gameObject.SetActive(show);
                }
            }
            if (show)
                ConsultingSetup.ValidateBoard();
        }

        public static void ConfirmArea(ConsultingSetup.Target target, BoardValidity validity)
        {
            MeshRenderer targetRenderer = null;
            switch (target)
            {
                case ConsultingSetup.Target.PlayerHand:
                    targetRenderer = Hand1.GetComponent<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.OpponentHand:
                    targetRenderer = Hand2.GetComponent<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.Field:
                    targetRenderer = Field.GetComponent<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.PlayerCollection:
                    targetRenderer = Collection1.GetComponent<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.OpponentCollection:
                    targetRenderer = Collection2.GetComponent<MeshRenderer>();
                    break;
            }
            if (targetRenderer)
            {
                switch (validity)
                {
                    case BoardValidity.Invalid:
                        targetRenderer.material.SetColor("_EmissionColor", _Invalid);
                        break;
                    case BoardValidity.Semivalid:
                        targetRenderer.material.SetColor("_EmissionColor", _Semivalid);
                        break;
                    case BoardValidity.Valid:
                        targetRenderer.material.SetColor("_EmissionColor", _Valid);
                        break;
                }
            }
        }

        public static void LoadGame()
        {
            Board.InfoUI.GetYakuList(0).BuildFromCards(new List<Card>(), Board.players[0].CollectedYaku);
            Board.InfoUI.GetYakuList(1).BuildFromCards(new List<Card>(), Board.players[1].CollectedYaku);

            if (Settings.AiMode == Settings.AIMode.Omniscient)
            {
                Board.Deck = Board.players[0].Hand
                    .Union(Board.players[1].Hand)
                    .Union(Board.Field)
                    .Union(Board.players[0].CollectedCards)
                    .Union(Board.players[1].CollectedCards)
                    .Union(Board.Deck)
                    .ToList();
            }
            else
            {
                List<Card> opponentHand = new List<Card>();
                while (opponentHand.Count != Board.players[0].Hand.Count - (P1Oya ? 0 : 1))
                    opponentHand.Add(Card.CreateInstance<Card>());
                Board.Deck = Board.players[0].Hand
                    .Union(opponentHand)
                    .Union(Board.Field)
                    .Union(Board.players[0].CollectedCards)
                    .Union(Board.players[1].CollectedCards)
                    .ToList();
                while (Board.Deck.Count != 48)
                    Board.Deck.Add(Card.CreateInstance<Card>());
            }

            Board.BuildDeck();

            int p1HandCount = Board.players[0].Hand.Count;
            int p2HandCount;
            if (Settings.AiMode == Settings.AIMode.Omniscient)
                p2HandCount = Board.players[1].Hand.Count;
            else
                p2HandCount = p1HandCount - (P1Oya ? 0 : 1);
            int fieldCount = Board.Field.Count;
            Board.players[0].Hand.Clear();
            Board.players[1].Hand.Clear();
            Board.Field.Clear();
            Board.BuildHands(p1HandCount, p2HandCount);
            Board.BuildField(fieldCount);

            List<Card> p1Collected = Board.players[0].CollectedCards;
            List<Card> p2Collected = Board.players[1].CollectedCards;
            Board.players[0].CollectedCards.Clear();
            Board.players[1].CollectedCards.Clear();
            Board.CollectCards(p1Collected);
            Board.Turn = false;
            Board.CollectCards(p2Collected);
            Board.Turn = true;

            MarkAreas(false, true);
        }
        public void Update()
        {
            if (Global.MovingCards > 0) return;
            if (MainSceneVariables.consultingTransforms.ConsultingBuilder.activeInHierarchy) return;
            if (MainSceneVariables.consultingTransforms.MoveBuilder.gameObject.activeInHierarchy) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5000f, LayerMask.GetMask("Consulting")))
            {
                if (Input.GetMouseButton(0))
                {
                    ConsultingSetup target = hit.collider.gameObject.GetComponent<ConsultingSetup>();
                    if (BoardBuilded)
                        target.SetupMove();
                    else
                        target.SetupArea();
                }
            }
        }
    }
}
