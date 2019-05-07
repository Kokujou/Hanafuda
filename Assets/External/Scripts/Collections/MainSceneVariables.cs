using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public class MainSceneVariables : MonoBehaviour
    {
        public static BoardTransforms boardTransforms;
        public static ConsultingTransforms consultingTransforms;

        // Use this for initialization
        public GameObject ToInstantiate;
        [SerializeField] public BoardTransforms BoardSingleton;
        [SerializeField] public ConsultingTransforms ConsultingSingleton;
        private void Start()
        {
            boardTransforms = BoardSingleton;
            consultingTransforms = ConsultingSingleton;
            if (ToInstantiate)
            {
                GameObject obj = Instantiate(ToInstantiate);
                if (Settings.Tutorial && Settings.Mobile)
                {
                    obj.AddComponent<Tutorial>();
                }
            }
            else
            {
                boardTransforms.Main = new GameObject("Board").AddComponent<Spielfeld>();
                if (Settings.Tutorial)
                    boardTransforms.Main.gameObject.AddComponent<Tutorial>();
                else if (Settings.Consulting)
                    boardTransforms.Main.gameObject.AddComponent<Consulting>();
            }
        }

        public void ResetBuilder(GameObject content)
        {
            foreach (Transform child in content.transform)
                Destroy(child.gameObject);
        }

        [Serializable]
        public class BoardTransforms
        {
            public Spielfeld Main;
            public Transform Hand1, Hand2, Hand1M, Hand2M, Feld, MFeld, Deck, MDeck, EffectCamera;
            [SerializeField] public List<Transform> PCCollections = new List<Transform>();
        }

        [Serializable]
        public class ConsultingTransforms
        {
            public Button SetupConfirm, MoveConfirm;
            public Transform SetupContent, HandSelection, HandFieldSelection, DeckSelection, DeckFieldSelection;
            public GameObject ConsultingBuilder, OyaSelection;
            public ConsultingMoveBuilder MoveBuilder;
            public ConsultingSetup Hand1, Hand2, Hand1M, Hand2M, Feld, MFeld, Collection1M, Collection2M, Collection1, Collection2;
            public Toggle P1Toggle;
        }
    }
}