using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hanafuda
{
    public class MessageBox : MonoBehaviour, IDragHandler
    {
        public GameObject Container;
        public Text Caption, Content;
        public RectTransform ButtonParent;
        public Func<bool> DestroyCallback = () => false;

        private void Destroy()
        {
            Destroy(transform.parent.gameObject);
        }

        private void Setup(string caption, string content)
        {
            Caption.text = caption;
            Content.text = content;
        }

        public void Setup(string caption, string content, params KeyValuePair<string, Action>[] buttons)
        {
            Setup(caption, content);
            foreach (KeyValuePair<string, Action> button in buttons)
            {
                Button obj = Instantiate(Global.prefabCollection.UIButton).GetComponent<Button>();
                obj.transform.SetParent(ButtonParent, true);
                obj.onClick.AddListener(() => button.Value());
                obj.onClick.AddListener(() => Destroy());
                obj.GetComponentInChildren<Text>().text = button.Key;
            }
        }

        public void Setup(string caption, string content, Func<bool> destroyCallback)
        {
            Setup(caption, content);
            DestroyCallback = destroyCallback;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Container)
                Container.transform.position += (Vector3)eventData.delta;
            else
                transform.position += (Vector3)eventData.delta;
        }

        private void Update()
        {
            if (DestroyCallback())
                Destroy(transform.parent.gameObject);
        }
    }
}