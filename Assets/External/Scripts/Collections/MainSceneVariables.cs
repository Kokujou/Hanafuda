using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public class MainSceneVariables : MonoBehaviour
    {
        public static VariableCollection variableCollection;

        // Use this for initialization
        public GameObject obj;

        [SerializeField] public VariableCollection singleton;

        private void Start()
        {
            variableCollection = singleton;
            Instantiate(obj);
        }

        [Serializable]
        public class VariableCollection
        {
            public float BoxX;
            public Transform ExCol, ExColBack, Hand1, Hand2, Hand1M, Hand2M, Feld, MFeld, Deck, MDeck, EffectCamera;
            [SerializeField] public List<Transform> PCCollections = new List<Transform>();
        }
    }
}