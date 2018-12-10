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
            for (var i = 0; i < variableCollection.PCCollections.Count; i++)
            {
                var name = (i % 5 != 0 ? variableCollection.PCCollections[i].parent.name : "") +
                           variableCollection.PCCollections[i].name;
                variableCollection.Collections.Add(name.GetHashCode(), variableCollection.PCCollections[i]);
            }

            Instantiate(obj);
        }

        [Serializable]
        public class VariableCollection
        {
            public float BoxX;
            public Hashtable Collections = new Hashtable();
            public Transform ExCol, ExColBack, Hand1, Hand2, Hand1M, Hand2M, Feld, MFeld, Deck, MDeck, EffectCamera;

            [SerializeField] public List<Transform> PCCollections = new List<Transform>();
        }
    }
}