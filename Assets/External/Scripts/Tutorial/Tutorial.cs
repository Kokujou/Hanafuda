using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public partial class Tutorial : MonoBehaviour
    {
        Negotiation NegMain;
        Spielfeld Board;
        GameObject Hide, Command, HandMask;
        private void Awake()
        {
            NegMain = GetComponent<Negotiation>();
            Board = GetComponent<Spielfeld>();
            if (NegMain)
            {
                InitNegotiation();
            }
            else if (Board)
            {
                InitBoard();
            }
        }
        private GameObject CreateUIMask(GameObject target, float offset)
        {
            List<Collider> childTransforms = new List<Collider>();
            childTransforms.AddRange(target.GetComponentsInChildren<Collider>());

            float left = 0, right = 0, top = 0, bottom = 0;
            for (int child = 0; child < childTransforms.Count; child++)
            {
                Collider renderer = childTransforms[child];
                float correctX = 0f, correctY = 0f;
                if (renderer.gameObject == target)
                {
                    correctX = renderer.transform.localPosition.x;
                    correctY = renderer.transform.localPosition.y;
                }
                if (renderer.bounds.min.x - correctX < left)
                    left = renderer.bounds.min.x - correctX;
                if (renderer.bounds.max.x - correctX > right)
                    right = renderer.bounds.max.x - correctX;
                if (renderer.bounds.min.y - correctY < top)
                    top = renderer.bounds.min.y - correctY;
                if (renderer.bounds.max.y - correctY > bottom)
                    bottom = renderer.bounds.max.y - correctY;
            }

            GameObject mask = Instantiate(Global.prefabCollection.UIMask, target.transform);
            float x = right - left, y = bottom - top;
            mask.transform.localScale = new Vector3((y) / mask.GetComponent<Renderer>().bounds.size.y + offset, x / mask.GetComponent<Renderer>().bounds.size.x + offset, 1);
            return mask;
        }
        private GameObject Create3DText(string caption)
        {
            GameObject text = Instantiate(Global.prefabCollection.PText);
            TextMesh[] texts = text.GetComponentsInChildren<TextMesh>();
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = caption;
                texts[i].alignment = TextAlignment.Center;
                texts[i].anchor = TextAnchor.MiddleCenter;
            }
            text.AddComponent<BoxCollider>();
            CreateUIMask(text, 3f).transform.localPosition += Vector3.down;

            return text;
        }
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}