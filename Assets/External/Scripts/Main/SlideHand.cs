using Hanafuda.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.EventSystems;

namespace Hanafuda
{
    public class SlideHand : MonoBehaviour
    {
        /*
         * Todo:
         * - Visualisierung der hover-bounds
         * - Hover klären
         */
        // Use this for initialization
        private int ToHover;
        private Action<int> OnSelect = null;
        private Action<int> OnHover = null;
        private int SelectedCard;
        private bool IsValid;
        private bool Initialized;

        private void Start()
        {
            StartCoroutine(gameObject.BlinkSlide());
        }

        public void Init(int toHover, Action<int> onHover, Action<int> onSelect)
        {
            ToHover = toHover;
            OnHover = onHover;
            OnSelect = onSelect;
            Initialized = true;
        }
        // Update is called once per frame
        private void Update()
        {
            if (!Initialized) return;
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
            if (ToHover != 0)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit) && hit.collider.name.StartsWith("Slide"))
                    IsValid = true;
                if (Input.GetMouseButton(0) && IsValid)
                {
                    int tempSelection = (int)((Camera.main.ScreenToWorldPoint(Input.mousePosition).x + 15f) / (30f / ToHover));
                    if (tempSelection == SelectedCard) return;
                    else SelectedCard = tempSelection;
                    if (SelectedCard < 0) SelectedCard = 0;
                    else if (SelectedCard >= ToHover) SelectedCard = ToHover - 1;
                    OnHover(SelectedCard);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    /*
                     * Toleranzerhöhung durch Entschärfung der Bedingung
                     */
                    if (Physics.Raycast(ray, out hit) && hit.collider.name.StartsWith("Slide"))
                    {
                        OnSelect(SelectedCard);
                        Destroy(gameObject);
                    }
                    else
                    {
                        gameObject.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 0.6f);
                        IsValid = false;
                        OnHover(-1);
                    }
                }
            }
        }
    }
}