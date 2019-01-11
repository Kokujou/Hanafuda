﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public class MainSceneVariables : MonoBehaviour
    {
        public static VariableCollection variableCollection;

        // Use this for initialization
        public GameObject ToInstantiate;
        [SerializeField] public VariableCollection singleton;
        private void Start()
        {
            variableCollection = singleton;
            if (ToInstantiate)
            {
                GameObject obj = Instantiate(ToInstantiate);
                if (Settings.Tutorial)
                {
                    obj.AddComponent<Tutorial>();
                }
            }
            else
            {
                variableCollection.Main = new GameObject("Board").AddComponent<Spielfeld>();
                if (Settings.Tutorial)
                {
                    variableCollection.Main.gameObject.AddComponent<Tutorial>();
                }
            }
        }

        [Serializable]
        public class VariableCollection
        {
            public float BoxX;
            public Spielfeld Main;
            public Transform ExCol, ExColBack, Hand1, Hand2, Hand1M, Hand2M, Feld, MFeld, Deck, MDeck, EffectCamera;
            [SerializeField] public List<Transform> PCCollections = new List<Transform>();
        }
    }
}