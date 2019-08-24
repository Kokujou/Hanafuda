using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hanafuda
{
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Card AssignedCard;

        private Vector2 startPosition;
        private Transform startParent;
        private GameObject dummy;
        private int startSiblingIndex;

        private CardDropContainer currentContainer;

        public void Start()
        {
            currentContainer = GetComponentInParent<CardDropContainer>();
            currentContainer.AddCard(AssignedCard);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = transform.position;
            startParent = transform.parent;
            startSiblingIndex = transform.GetSiblingIndex();

            CreateDummyGridCell();
            transform.SetParent(EventSystem.current.transform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (startPosition == Vector2.zero)
                startPosition = transform.position;

            transform.position += (Vector3)eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var rayCastMatches = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, rayCastMatches);
            var target = GetFirstContainer(rayCastMatches.Select(x => x.gameObject));

            if (DropAllowed(target))
                ReassignCard(target);
            else
                ResetCard();
        }

        private bool DropAllowed(CardDropContainer target)
            => target
            && target != currentContainer;

        private void ReassignCard(CardDropContainer container)
        {
            Destroy(dummy);
            currentContainer.RemoveCard(AssignedCard);
            currentContainer = container;
            container.ReceiveCard(gameObject, AssignedCard);
        }

        private void ResetCard()
        {
            transform.position = startPosition;
            transform.SetParent(startParent);
            transform.SetSiblingIndex(startSiblingIndex);

            Destroy(dummy);
        }

        private CardDropContainer GetFirstContainer(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                var dropContainer = obj.GetComponent<CardDropContainer>();
                if (dropContainer)
                    return dropContainer;
            }
            return null;
        }

        private void CreateDummyGridCell()
        {
            dummy = new GameObject();
            dummy.AddComponent<RectTransform>();
            dummy.transform.SetParent(startParent);
            dummy.transform.SetSiblingIndex(startSiblingIndex);
        }
    }
}
