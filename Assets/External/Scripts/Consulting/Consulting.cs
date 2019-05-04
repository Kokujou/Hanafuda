using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public partial class Spielfeld : ISpielfeld
    {
        private static Color _Valid = new Color(0, .5f, 0);
        private static Color _Invalid = new Color(.5f, 0, 0);
        private static Color _Semivalid = new Color(.5f, .5f, 0);

        public override void InitConsulting()
        {
            Settings.Players[1 - Settings.PlayerID] = KI.Init((KI.Mode)Settings.KIMode, "Computer");
            players = Settings.Players;
            Deck = new List<Card>(Global.allCards);
            Field = new List<Card>();
            Button confirm = Hand1.GetComponentInChildren<ConsultingSetup>(true)
                    .Builder.GetComponentInChildren<Button>();
            confirm.onClick.AddListener(
                () => { ConsultingSetup.ValidateBoard(); Global.MovingCards--; });
            MarkAreas();
        }

        public override void MarkAreas(bool show = true)
        {
            Hand1.GetComponentInChildren<ConsultingSetup>(show).BoardBuilded = !show;
            Hand2.GetComponentInChildren<ConsultingSetup>(show).BoardBuilded = !show;

            Hand1.GetComponentInChildren<ConsultingSetup>(show).gameObject.SetActive(true);
            Hand2.GetComponentInChildren<ConsultingSetup>(show).gameObject.SetActive(show);

            Field3D.GetComponentInChildren<ConsultingSetup>(show).gameObject.SetActive(show);
            if (!Settings.Mobile)
            {
                MainSceneVariables.variableCollection.PCCollections[0]
                    .parent.GetComponentInChildren<ConsultingSetup>(show)
                    .gameObject.SetActive(show);
                MainSceneVariables.variableCollection.PCCollections[4]
                    .parent.GetComponentInChildren<ConsultingSetup>(show)
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

        public override void ConfirmArea(ConsultingSetup.Target target, BoardValidity validity)
        {
            MeshRenderer targetRenderer = null;
            switch (target)
            {
                case ConsultingSetup.Target.PlayerHand:
                    targetRenderer = Hand1.GetComponentInChildren<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.OpponentHand:
                    targetRenderer = Hand2.GetComponentInChildren<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.Field:
                    targetRenderer = Field3D.GetComponentInChildren<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.PlayerCollection:
                    if (!Settings.Mobile)
                        targetRenderer = MainSceneVariables.variableCollection.PCCollections[0]
                            .parent.GetComponentInChildren<MeshRenderer>();
                    else
                        targetRenderer = FindObjectsOfType<MobileContainerArea>()
                            .Where(x => x.GetComponent<ConsultingSetup>().SetupTarget == target)
                            .First().GetComponent<MeshRenderer>();
                    break;
                case ConsultingSetup.Target.OpponentCollection:
                    if (!Settings.Mobile)
                        targetRenderer = MainSceneVariables.variableCollection.PCCollections[4]
                            .parent.GetComponentInChildren<MeshRenderer>();
                    else
                        targetRenderer = FindObjectsOfType<MobileContainerArea>()
                                .Where(x => x.GetComponent<ConsultingSetup>().SetupTarget == target)
                                .First().GetComponent<MeshRenderer>();
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

        public override void LoadGame()
        {
            InfoUI.GetYakuList(0).BuildFromCards(new List<Card>(), players[0].CollectedYaku);
            InfoUI.GetYakuList(1).BuildFromCards(new List<Card>(), players[1].CollectedYaku);
            Deck = players[0].Hand
                .Union(players[1].Hand)
                .Union(Field)
                .Union(players[0].CollectedCards)
                .Union(players[1].CollectedCards)
                .Union(Deck)
                .ToList();
            BuildDeck();
            BuildHands(players[0].Hand.Count, players[1].Hand.Count);
            BuildField(Field.Count);
                CollectCards(players[0].CollectedCards);
                Turn = false;
                CollectCards(players[1].CollectedCards);
                Turn = true;
            MarkAreas(false);
            Global.MovingCards--;
        }
        public void Update()
        {
            if (Global.MovingCards > 0) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5000f, LayerMask.GetMask("Consulting")))
            {
                if (Input.GetMouseButton(0))
                {
                    ConsultingSetup target = hit.collider.gameObject.GetComponent<ConsultingSetup>();
                    if (target.BoardBuilded)
                        target.SetupMove();
                    else
                        target.SetupArea();
                }
            }
        }
    }
}
