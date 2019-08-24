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

        // Use this for initialization
        public GameObject ToInstantiate;
        [SerializeField] public BoardTransforms BoardSingleton;
        private void Start()
        {
            boardTransforms = BoardSingleton;
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
    }
}