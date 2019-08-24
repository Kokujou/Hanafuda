using System;
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

        private List<Card> Inventory = new List<Card>();

        private void Start()
        {
            if (!Container)
                Container = transform.parent;
        }

        public void ReceiveCard(GameObject cardObject, Card cardInstance)
        {
            Debug.Log("Received Card");
            AssignGameObject(cardObject);
            AddCard(cardInstance);
        }

        public void AddCard(Card card)
        {
            Inventory.Add(card);
        }

        public void RemoveCard(Card cardInstance)
        {
            Inventory.Remove(cardInstance);
        }

        public List<Card> GetInventory()
            => Inventory.ToList();

        private void AssignGameObject(GameObject cardObject)
        {
            cardObject.transform.SetParent(Container);
        }

        
    }
}
