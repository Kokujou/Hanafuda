using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public List<Card> cParent = new List<Card>();
        public Action<Card> onComplete = null;
        public Card selected;
        private int selectedCard;
        public List<Card> toHover = new List<Card>();
        private bool valid;

        private void Start()
        {
            StartCoroutine(Global.BlinkSlide(gameObject));
        }

        // Update is called once per frame
        private void Update()
        {
            if (cParent.Count != 0)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                var Parent = gameObject.transform.parent.gameObject;
                if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit) &&
                    hit.collider.name.StartsWith("Slide") &&
                    (int) ((hit.point.x + 15f) / (30f / cParent.Count)) != selectedCard) valid = true;
                if (Input.GetMouseButton(0) && valid)
                {
                    // Visualisierung der hover-Bounds
                    //GameObject.Find("Slide(Clone)").GetComponent<SpriteRenderer>().color *= new Color(1, 1, 1, 0);
                    if (Global.prev)
                    {
                        Global.UnhoverCard(Global.prev);
                        Global.prev = null;
                    }

                    selectedCard = (int) ((Camera.main.ScreenToWorldPoint(Input.mousePosition).x + 15f) /
                                          (30f / cParent.Count));
                    if (selectedCard < 0) selectedCard = 0;
                    else if (selectedCard >= cParent.Count) selectedCard = cParent.Count - 1;
                    Global.HoverCard(cParent[selectedCard].Objekt.GetComponent<BoxCollider>());
                    for (var i = 0; i < toHover.Count; i++)
                    {
                        Color col;
                        if (toHover[i].Monat != cParent[selectedCard].Monat)
                            col = new Color(.3f, .3f, .3f);
                        else
                            col = new Color(.5f, .5f, .5f);
                        toHover[i].Objekt.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Foreground")
                            .material.SetColor("_TintColor", col);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (Physics.Raycast(ray, out hit) && hit.collider.name.StartsWith("Slide"))
                    {
                        selected = cParent[selectedCard];
                        if (toHover.Count(x => x.Monat == cParent[selectedCard].Monat) != 2)
                            for (var i = 0; i < toHover.Count; i++)
                                toHover[i].Objekt.GetComponentsInChildren<MeshRenderer>()
                                    .First(x => x.name == "Foreground").material
                                    .SetColor("_TintColor", new Color(.5f, .5f, .5f));
                        onComplete(selected);
                        Destroy(gameObject);
                    }
                    else
                    {
                        gameObject.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 0.6f);
                        if (Global.prev)
                        {
                            Global.UnhoverCard(Global.prev);
                            Global.prev = null;
                        }

                        for (var i = 0; i < toHover.Count; i++)
                        {
                            var col = new Color(.5f, .5f, .5f);
                            toHover[i].Objekt.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Foreground")
                                .material.SetColor("_TintColor", col);
                        }

                        valid = false;
                    }
                }
            }
        }
    }
}