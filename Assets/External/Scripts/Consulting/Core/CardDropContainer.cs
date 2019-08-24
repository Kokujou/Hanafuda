﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public class CardDropContainer : MonoBehaviour
    {
        public Transform Container;
        public int MaxSize;

        public List<Card> Inventory = new List<Card>();

        private void Start()
        {
            if (!Container)
                Container = transform.parent;
        }

        public void ReceiveCard(GameObject cardObject, Card cardInstance)
        {
            Debug.Log("Received Card");
            AssignGameObject(cardObject);
            Inventory.Add(cardInstance);
        }

        private void AssignGameObject(GameObject cardObject)
        {
            cardObject.transform.SetParent(Container, false);
            cardObject.transform.localScale = Vector3.one;
        }
    }
}
