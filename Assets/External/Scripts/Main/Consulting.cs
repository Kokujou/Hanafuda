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
            MarkAreas();
        }

        public override void MarkAreas()
        {
            Hand1.GetComponentInChildren<ConsultingSetup>(true).gameObject.SetActive(true);
            Hand1.GetComponentInChildren<ConsultingSetup>()
                .Builder.GetComponentInChildren<Button>().onClick.AddListener(
                () => { ConsultingSetup.ValidateBoard(); Global.MovingCards--; });
            Hand2.GetComponentInChildren<ConsultingSetup>(true).gameObject.SetActive(true);
            Field3D.GetComponentInChildren<ConsultingSetup>(true).gameObject.SetActive(true);
            if (!Settings.Mobile)
            {
                MainSceneVariables.variableCollection.PCCollections[0]
                    .parent.GetComponentInChildren<ConsultingSetup>(true)
                    .gameObject.SetActive(true);
                MainSceneVariables.variableCollection.PCCollections[4]
                    .parent.GetComponentInChildren<ConsultingSetup>(true)
                    .gameObject.SetActive(true);
            }
        }

        public override void ConfirmArea(ConsultingSetup.Target target, bool confirm = true)
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
                    break;
                case ConsultingSetup.Target.OpponentCollection:
                    if (!Settings.Mobile)
                        targetRenderer = MainSceneVariables.variableCollection.PCCollections[4]
                            .parent.GetComponentInChildren<MeshRenderer>();
                    break;
            }
            if (targetRenderer)
            {
                if (!confirm)
                    targetRenderer.material.SetColor("_EmissionColor", _Invalid);
                else if (targetRenderer.transform.parent.childCount == 1)
                    targetRenderer.material.SetColor("_EmissionColor", _Semivalid);
                else
                    targetRenderer.material.SetColor("_EmissionColor", _Valid);
            }
        }

        public void LoadGame()
        {
            InfoUI.GetYakuList(0).BuildFromCards(new List<Card>(), players[0].CollectedYaku);
            InfoUI.GetYakuList(1).BuildFromCards(new List<Card>(), players[1].CollectedYaku);
            Deck = Deck.Union(players[0].Hand)
                .Union(players[1].Hand)
                .Union(Field)
                .ToList();
            BuildDeck();
            BuildHands(players[0].Hand.Count, players[1].Hand.Count);
            BuildField(Field.Count);
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
                    target.Setup();
                }
            }
        }
    }
}
