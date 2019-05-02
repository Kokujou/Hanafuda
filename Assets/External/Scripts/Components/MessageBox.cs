using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hanafuda
{
    [RequireComponent(typeof(EventTrigger))]
    public class MessageBox : MonoBehaviour
    {
        public Text Caption, Content;
        public RectTransform ButtonParent;
        public Func<bool> DestroyCallback = () => false;

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
                obj.transform.SetParent(ButtonParent,true);
                obj.onClick.AddListener(() => button.Value());
                obj.GetComponentInChildren<Text>().text = button.Key;
            }
        }

        public void Setup(string caption, string content, Func<bool> destroyCallback)
        {
            Setup(caption, content);
            DestroyCallback = destroyCallback;
        }

        public void OnDrag(BaseEventData data)
        {
            PointerEventData eventData = (PointerEventData)data;
            GetComponent<RectTransform>().localPosition += (Vector3)eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
        }

        private void Update()
        {
            if (DestroyCallback())
                Destroy(transform.parent.gameObject);
        }
    }
}